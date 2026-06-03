import React, { useCallback, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  KeyboardAvoidingView,
  Platform,
  RefreshControl,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { BorderRadius, Colors, Spacing, Typography } from '../../constants/theme';
import { useToast } from '../../contexts/ToastContext';
import { messageApi } from '../../api/messageApi';
import { MessageDto } from '../../types/message';
import { TextInput } from '../../components/TextInput';
import { messageHubService } from '../../services/messageHub';

export const ConversationDetailScreen: React.FC<any> = ({ route }) => {
  const { showToast } = useToast();
  const conversationId = route?.params?.conversationId as number;

  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [sending, setSending] = useState(false);
  const [draft, setDraft] = useState('');

  const loadMessages = useCallback(async (isRefresh = false) => {
    try {
      if (isRefresh) setRefreshing(true);
      else setLoading(true);

      const data = await messageApi.getMessages(conversationId, 1, 50);
      setMessages(data);
      await messageApi.markAsRead(conversationId);
    } catch (error: any) {
      showToast(error?.response?.data?.message || 'Failed to load messages', 'error');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [conversationId, showToast]);

  useFocusEffect(
    useCallback(() => {
      loadMessages();

      const unsubscribe = messageHubService.subscribe((incoming) => {
        if (incoming.conversationId !== conversationId) {
          return;
        }

        setMessages((prev) => {
          if (prev.some((m) => m.id === incoming.id)) {
            return prev;
          }
          return [incoming, ...prev];
        });

        messageApi.markAsRead(conversationId).catch(() => {
          // Ignore mark-as-read failures for realtime updates.
        });
      });

      messageHubService.ensureConnected().catch(() => {
        // Keep normal polling/refresh behavior when realtime connect fails.
      });

      return () => {
        unsubscribe();
      };
    }, [loadMessages])
  );

  const sendDisabled = useMemo(() => sending || !draft.trim(), [sending, draft]);

  const handleSend = async () => {
    if (sendDisabled) return;

    try {
      setSending(true);
      await messageApi.sendMessage(conversationId, { content: draft.trim() });
      setDraft('');
      await loadMessages(true);
    } catch (error: any) {
      showToast(error?.response?.data?.message || 'Failed to send message', 'error');
    } finally {
      setSending(false);
    }
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primaryBlue} />
        <Text style={styles.loadingText}>Loading conversation...</Text>
      </View>
    );
  }

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      keyboardVerticalOffset={90}
    >
      <FlatList
        data={messages}
        keyExtractor={(item) => item.id.toString()}
        contentContainerStyle={styles.listContent}
        renderItem={({ item }) => (
          <View style={[styles.messageBubble, item.isMine ? styles.myBubble : styles.otherBubble]}>
            <Text style={[styles.messageText, item.isMine ? styles.myText : styles.otherText]}>
              {item.content}
            </Text>
            <Text style={[styles.messageMeta, item.isMine ? styles.myMeta : styles.otherMeta]}>
              {new Date(item.createdAt).toLocaleString()}
            </Text>
          </View>
        )}
        ListEmptyComponent={<Text style={styles.emptyText}>No messages yet.</Text>}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => loadMessages(true)}
            colors={[Colors.primaryBlue]}
            tintColor={Colors.primaryBlue}
          />
        }
      />

      <View style={styles.composerWrap}>
        <TextInput
          value={draft}
          onChangeText={setDraft}
          placeholder="Type a message..."
          maxLength={2000}
          containerStyle={styles.inputContainer}
        />
        <TouchableOpacity
          style={[styles.sendButton, sendDisabled && styles.sendButtonDisabled]}
          onPress={handleSend}
          disabled={sendDisabled}
        >
          {sending ? <ActivityIndicator size="small" color={Colors.white} /> : <Text style={styles.sendText}>Send</Text>}
        </TouchableOpacity>
      </View>
    </KeyboardAvoidingView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.gray50 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  loadingText: {
    marginTop: Spacing.sm,
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodyMd,
  },
  listContent: { padding: Spacing.base, paddingBottom: Spacing['2xl'] },
  emptyText: {
    textAlign: 'center',
    color: Colors.gray600,
    marginTop: Spacing.lg,
    fontFamily: Typography.fontFamily.medium,
  },
  messageBubble: {
    maxWidth: '84%',
    borderRadius: BorderRadius.lg,
    paddingHorizontal: Spacing.md,
    paddingVertical: Spacing.sm,
    marginBottom: Spacing.sm,
  },
  myBubble: { alignSelf: 'flex-end', backgroundColor: Colors.primaryBlue },
  otherBubble: { alignSelf: 'flex-start', backgroundColor: Colors.white, borderWidth: 1, borderColor: Colors.gray200 },
  messageText: { fontSize: Typography.size.bodyMd, fontFamily: Typography.fontFamily.regular },
  myText: { color: Colors.white },
  otherText: { color: Colors.gray900 },
  messageMeta: { marginTop: 4, fontSize: Typography.size.caption, fontFamily: Typography.fontFamily.regular },
  myMeta: { color: '#E7EEF7' },
  otherMeta: { color: Colors.gray600 },
  composerWrap: {
    borderTopWidth: 1,
    borderTopColor: Colors.gray200,
    backgroundColor: Colors.white,
    padding: Spacing.base,
    flexDirection: 'row',
    alignItems: 'flex-end',
    gap: Spacing.sm,
  },
  inputContainer: { flex: 1, marginBottom: 0 },
  sendButton: {
    height: 50,
    minWidth: 72,
    backgroundColor: Colors.primaryBlue,
    borderRadius: BorderRadius.md,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: Spacing.md,
  },
  sendButtonDisabled: { opacity: 0.6 },
  sendText: {
    color: Colors.white,
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.labelLg,
  },
});
