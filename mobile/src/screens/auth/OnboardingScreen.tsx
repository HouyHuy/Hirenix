/**
 * OnboardingScreen — Hirenix
 * 3 slides giới thiệu app, skip-able, dot indicator
 */
import React, { useRef, useState } from 'react';
import {
  View, Text, StyleSheet, Dimensions, FlatList,
  Animated, StatusBar, TouchableOpacity,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons } from '@expo/vector-icons';
import { Colors, Typography, Spacing } from '../../constants/theme';
import { Button } from '../../components/Button';

const { width } = Dimensions.get('window');

interface OnboardingScreenProps { onFinish: () => void; }

interface SlideData {
  id: string;
  icon: keyof typeof Ionicons.glyphMap;
  iconBgColors: readonly [string, string, ...string[]];
  title: string;
  description: string;
  accent: string;
}

const slides: SlideData[] = [
  {
    id: '1', icon: 'search-outline',
    iconBgColors: ['#1A6FBF', '#2D8BC9'],
    title: 'Tìm việc dễ dàng',
    description: 'Hàng nghìn vị trí từ các công ty hàng đầu. Bộ lọc thông minh giúp bạn tìm đúng công việc mơ ước.',
    accent: Colors.primaryBlue,
  },
  {
    id: '2', icon: 'paper-plane-outline',
    iconBgColors: ['#1D9E75', '#2DBFA0'],
    title: 'Ứng tuyển nhanh chóng',
    description: 'Chỉ cần một chạm để nộp hồ sơ. Theo dõi trạng thái đơn ứng tuyển theo thời gian thực.',
    accent: Colors.accentTeal,
  },
  {
    id: '3', icon: 'chatbubbles-outline',
    iconBgColors: ['#D97706', '#F59E0B'],
    title: 'Kết nối trực tiếp',
    description: 'Chat trực tiếp với nhà tuyển dụng. Nhận thông báo lịch phỏng vấn và offer ngay trên app.',
    accent: Colors.warningAmber,
  },
];

export const OnboardingScreen: React.FC<OnboardingScreenProps> = ({ onFinish }) => {
  const [currentIndex, setCurrentIndex] = useState(0);
  const scrollX = useRef(new Animated.Value(0)).current;
  const flatListRef = useRef<FlatList>(null);

  const onViewRef = useRef(({ viewableItems }: any) => {
    if (viewableItems.length > 0) setCurrentIndex(viewableItems[0].index ?? 0);
  });
  const viewConfigRef = useRef({ viewAreaCoveragePercentThreshold: 50 });

  const handleNext = () => {
    if (currentIndex < slides.length - 1) {
      flatListRef.current?.scrollToIndex({ index: currentIndex + 1 });
    } else {
      onFinish();
    }
  };

  const renderSlide = ({ item, index }: { item: SlideData; index: number }) => {
    const inputRange = [(index - 1) * width, index * width, (index + 1) * width];
    const iconTranslateY = scrollX.interpolate({ inputRange, outputRange: [40, 0, -40] });
    const iconOpacity = scrollX.interpolate({ inputRange, outputRange: [0, 1, 0] });
    const textTranslateX = scrollX.interpolate({ inputRange, outputRange: [60, 0, -60] });
    const textOpacity = scrollX.interpolate({ inputRange, outputRange: [0, 1, 0] });

    return (
      <View style={[styles.slide, { width }]}>
        <View style={styles.illustrationArea}>
          <View style={[styles.decorRing, { borderColor: `${item.accent}15` }]} />
          <Animated.View style={{ transform: [{ translateY: iconTranslateY }], opacity: iconOpacity }}>
            <LinearGradient colors={item.iconBgColors} style={styles.iconGradient} start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }}>
              <Ionicons name={item.icon} size={64} color={Colors.white} />
            </LinearGradient>
          </Animated.View>
        </View>
        <Animated.View style={[styles.textContent, { transform: [{ translateX: textTranslateX }], opacity: textOpacity }]}>
          <Text style={styles.slideTitle}>{item.title}</Text>
          <Text style={styles.slideDescription}>{item.description}</Text>
        </Animated.View>
      </View>
    );
  };

  return (
    <View style={styles.container}>
      <StatusBar barStyle="dark-content" backgroundColor={Colors.gray50} />
      {currentIndex < slides.length - 1 && (
        <TouchableOpacity style={styles.skipButton} onPress={onFinish} activeOpacity={0.7}>
          <Text style={styles.skipText}>Bỏ qua</Text>
        </TouchableOpacity>
      )}
      <Animated.FlatList
        ref={flatListRef} data={slides} renderItem={renderSlide}
        keyExtractor={(item: SlideData) => item.id} horizontal pagingEnabled
        showsHorizontalScrollIndicator={false}
        onScroll={Animated.event([{ nativeEvent: { contentOffset: { x: scrollX } } }], { useNativeDriver: false })}
        onViewableItemsChanged={onViewRef.current}
        viewabilityConfig={viewConfigRef.current} bounces={false}
      />
      <View style={styles.bottomArea}>
        <View style={styles.dotRow}>
          {slides.map((slide, index) => {
            const dotWidth = scrollX.interpolate({
              inputRange: [(index - 1) * width, index * width, (index + 1) * width],
              outputRange: [8, 28, 8], extrapolate: 'clamp',
            });
            const dotOpacity = scrollX.interpolate({
              inputRange: [(index - 1) * width, index * width, (index + 1) * width],
              outputRange: [0.3, 1, 0.3], extrapolate: 'clamp',
            });
            return (
              <Animated.View key={slide.id} style={[styles.dot, {
                width: dotWidth, opacity: dotOpacity,
                backgroundColor: index === currentIndex ? slides[currentIndex].accent : Colors.gray400,
              }]} />
            );
          })}
        </View>
        <Button
          title={currentIndex === slides.length - 1 ? 'Bắt đầu ngay' : 'Tiếp theo'}
          onPress={handleNext}
        />
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.gray50 },
  skipButton: { position: 'absolute', top: 56, right: Spacing.base, zIndex: 10, paddingVertical: Spacing.sm, paddingHorizontal: Spacing.md },
  skipText: { fontFamily: Typography.fontFamily.medium, fontSize: Typography.size.bodyMd, color: Colors.gray600 },
  slide: { flex: 1, justifyContent: 'center', alignItems: 'center', paddingHorizontal: Spacing.xl },
  illustrationArea: { width: 240, height: 240, alignItems: 'center', justifyContent: 'center', marginBottom: Spacing['2xl'] },
  decorRing: { position: 'absolute', width: 240, height: 240, borderRadius: 120, borderWidth: 2 },
  iconGradient: { width: 130, height: 130, borderRadius: 36, alignItems: 'center', justifyContent: 'center' },
  textContent: { alignItems: 'center', paddingHorizontal: Spacing.base },
  slideTitle: { fontFamily: Typography.fontFamily.bold, fontSize: Typography.size.displayMd, lineHeight: Typography.lineHeight.displayMd, color: Colors.gray900, textAlign: 'center', marginBottom: Spacing.md },
  slideDescription: { fontFamily: Typography.fontFamily.regular, fontSize: Typography.size.bodyLg, lineHeight: Typography.lineHeight.bodyLg, color: Colors.gray600, textAlign: 'center' },
  bottomArea: { paddingHorizontal: Spacing.base, paddingBottom: Spacing['4xl'], gap: Spacing.xl },
  dotRow: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: Spacing.sm },
  dot: { height: 8, borderRadius: 4 },
});
