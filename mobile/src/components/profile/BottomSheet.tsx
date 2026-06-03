import React, { useCallback, useEffect, useRef, useState } from 'react';
import {
    Animated,
    Dimensions,
    Keyboard,
    KeyboardEvent,
    Modal,
    PanResponder,
    Platform,
    Pressable,
    ScrollView,
    StyleSheet,
    Text,
    View,
    ViewStyle,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Colors, BorderRadius, Spacing, Typography, Shadows } from '../../constants/theme';

type BottomSheetProps = {
    visible: boolean;
    onClose: () => void;
    title?: string;
    children: React.ReactNode;
    snapToBottom?: boolean;
    sheetStyle?: ViewStyle;
};

const { height: SCREEN_HEIGHT } = Dimensions.get('window');
const DRAG_CLOSE_THRESHOLD = 80;
const VELOCITY_CLOSE_THRESHOLD = 0.5;

// â”€â”€ Backdrop tÃ¡ch riÃªng + memo â†’ khÃ´ng re-render khi sheet kÃ©o â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
const Backdrop = React.memo(({
    opacity,
    visible,
    onPress,
}: {
    opacity: Animated.Value;
    visible: boolean;
    onPress: () => void;
}) => (
    <Animated.View
        style={[styles.backdrop, { opacity }]}
        pointerEvents={visible ? 'auto' : 'none'}
    >
        <Pressable style={StyleSheet.absoluteFill} onPress={onPress} />
    </Animated.View>
));

export const BottomSheet: React.FC<BottomSheetProps> = ({
    visible,
    onClose,
    title,
    children,
    snapToBottom = true,
    sheetStyle,
}) => {
    const insets = useSafeAreaInsets();

    // translateY: open/close animation
    const translateY    = useRef(new Animated.Value(SCREEN_HEIGHT)).current;
    // dragY: chá»‰ dÃ¹ng khi kÃ©o handle
    const dragY         = useRef(new Animated.Value(0)).current;
    // backdropOpacity: chá»‰ thay Ä‘á»•i khi open/close, KHÃ”NG thay Ä‘á»•i khi kÃ©o
    const backdropOpacity = useRef(new Animated.Value(0)).current;

    // combinedY = translateY + dragY, dÃ¹ng native driver
    const combinedY = useRef(Animated.add(translateY, dragY)).current;

    const [bottomInset, setBottomInset] = useState(0);
    const scrollViewRef = useRef<ScrollView>(null);

    // â”€â”€ Open / Close â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    useEffect(() => {
        if (visible) {
            translateY.setValue(SCREEN_HEIGHT);
            dragY.setValue(0);
            backdropOpacity.setValue(0);
            setBottomInset(0);

            Animated.parallel([
                Animated.spring(translateY, {
                    toValue: 0,
                    useNativeDriver: true,
                    tension: 65,
                    friction: 11,
                }),
                Animated.timing(backdropOpacity, {
                    toValue: 1,
                    duration: 200,
                    useNativeDriver: true,
                }),
            ]).start();
        } else {
            setBottomInset(0);
        }
    }, [visible]);

    const animateClose = useCallback(() => {
        Keyboard.dismiss();
        Animated.parallel([
            Animated.timing(backdropOpacity, {
                toValue: 0,
                duration: 200,
                useNativeDriver: true,
            }),
            Animated.timing(translateY, {
                toValue: SCREEN_HEIGHT,
                duration: 280,
                useNativeDriver: true,
            }),
            Animated.timing(dragY, {
                toValue: 0,
                duration: 280,
                useNativeDriver: true,
            }),
        ]).start(() => onClose());
    }, [onClose]);

    // â”€â”€ PanResponder â€” Animated.event map gesture â†’ dragY trá»±c tiáº¿p
    // KhÃ´ng dÃ¹ng setValue trong callback â†’ khÃ´ng trigger re-render ProfileScreen
    const panResponder = useRef(
        PanResponder.create({
            onStartShouldSetPanResponder: () => true,
            onMoveShouldSetPanResponder: (_, gs) => gs.dy > 5,

            onPanResponderMove: (_, gs) => {
                // Chặn kéo lên
                dragY.setValue(Math.max(gs.dy, 0));
            },

            onPanResponderRelease: (_, gs) => {
                if (gs.dy > DRAG_CLOSE_THRESHOLD || gs.vy > VELOCITY_CLOSE_THRESHOLD) {
                    // Háº¡ bÃ n phÃ­m xuá»‘ng mÆ°á»£t mÃ  khi vuá»‘t Ä‘Ã³ng
                    Keyboard.dismiss();

                    // ÄÃ³ng â€” backdrop fade out nhanh
                    Animated.parallel([
                        Animated.timing(backdropOpacity, {
                            toValue: 0,
                            duration: 200,
                            useNativeDriver: true,
                        }),
                        Animated.timing(dragY, {
                            toValue: SCREEN_HEIGHT,
                            duration: 260,
                            useNativeDriver: true,
                        }),
                    ]).start(() => {
                        onClose();
                    });
                } else {
                    // Snap vá»
                    Animated.spring(dragY, {
                        toValue: 0,
                        useNativeDriver: true,
                        tension: 80,
                        friction: 10,
                    }).start();
                }
            },
        })
    ).current;

    // â”€â”€ Keyboard listeners â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    useEffect(() => {
        if (!visible) return;

        const showEvent = Platform.OS === 'ios' ? 'keyboardWillShow' : 'keyboardDidShow';
        const hideEvent = Platform.OS === 'ios' ? 'keyboardWillHide' : 'keyboardDidHide';

        const onShow = (e: KeyboardEvent) => setBottomInset(e.endCoordinates.height);
        const onHide = () => setBottomInset(0);

        const subShow = Keyboard.addListener(showEvent, onShow);
        const subHide = Keyboard.addListener(hideEvent, onHide);

        return () => {
            subShow.remove();
            subHide.remove();
        };
    }, [visible]);

    const maxSheetHeight = SCREEN_HEIGHT - bottomInset - insets.top - 16;
    const paddingBottom  = bottomInset > 0
        ? (Spacing.md as number)
        : Math.max(insets.bottom, Spacing['2xl'] as number);

    return (
        <Modal
            visible={visible}
            transparent
            animationType="none"
            onRequestClose={animateClose}
            statusBarTranslucent
        >
            {/* Backdrop â€” memo, khÃ´ng re-render khi dragY thay Ä‘á»•i */}
            <Backdrop
                opacity={backdropOpacity}
                visible={visible}
                onPress={animateClose}
            />

            {/* Sheet */}
            <View
                style={[styles.screenContainer, { bottom: bottomInset }]}
                pointerEvents="box-none"
            >
                <Animated.View
                    style={[
                        styles.sheetWrapper,
                        { transform: [{ translateY: combinedY }] },
                    ]}
                >
                    <View
                        style={[
                            styles.sheet,
                            Shadows.elevation2,
                            { maxHeight: maxSheetHeight, paddingBottom },
                            sheetStyle,
                        ]}
                    >
                        {/* Handle vá»›i PanResponder */}
                        <View style={styles.handleRow} {...panResponder.panHandlers}>
                            <View style={styles.handle} />
                        </View>

                        {title ? (
                            <Text style={styles.title}>{title}</Text>
                        ) : null}

                        <ScrollView
                            ref={scrollViewRef}
                            keyboardShouldPersistTaps="handled"
                            keyboardDismissMode="interactive"
                            showsVerticalScrollIndicator={false}
                            bounces={false}
                            contentContainerStyle={styles.content}
                        >
                            {children}
                        </ScrollView>
                    </View>
                </Animated.View>
            </View>
        </Modal>
    );
};

const styles = StyleSheet.create({
    backdrop: {
        ...StyleSheet.absoluteFillObject,
        backgroundColor: Colors.backdrop,
    },
    screenContainer: {
        position: 'absolute',
        left: 0,
        right: 0,
        justifyContent: 'flex-end',
    },
    sheetWrapper: {
        width: '100%',
    },
    sheet: {
        width: '100%',
        backgroundColor: Colors.white,
        borderTopLeftRadius: BorderRadius.xl,
        borderTopRightRadius: BorderRadius.xl,
    },
    handleRow: {
        paddingTop: Spacing.sm,
        paddingBottom: Spacing.md,
        alignItems: 'center',
        minHeight: 36,
    },
    handle: {
        width: 36,
        height: 4,
        backgroundColor: Colors.gray200,
        borderRadius: 2,
    },
    title: {
        fontFamily: Typography.fontFamily.semibold,
        fontSize: Typography.size.headingSm,
        lineHeight: Typography.lineHeight.headingSm,
        color: Colors.gray900,
        paddingHorizontal: Spacing.base,
        paddingBottom: Spacing.sm,
    },
    content: {
        paddingHorizontal: Spacing.base,
        paddingTop: Spacing.sm,
        paddingBottom: Spacing.lg,
    },
});
