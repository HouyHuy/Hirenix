/**
 * SplashScreen — Hirenix
 * Logo centered on brand blue background with animated loading
 * Auto-navigates after 2.5 seconds
 */
import React, { useEffect, useRef } from 'react';
import {
  View,
  Text,
  StyleSheet,
  Animated,
  Easing,
  Dimensions,
  StatusBar,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { Colors, Typography, Spacing } from '../../constants/theme';

const { width, height } = Dimensions.get('window');

interface SplashScreenProps {
  onFinish: () => void;
}

export const SplashScreen: React.FC<SplashScreenProps> = ({ onFinish }) => {
  // ─── Animations ───
  const logoScale = useRef(new Animated.Value(0.3)).current;
  const logoOpacity = useRef(new Animated.Value(0)).current;
  const taglineOpacity = useRef(new Animated.Value(0)).current;
  const taglineTranslateY = useRef(new Animated.Value(20)).current;
  const dotScale1 = useRef(new Animated.Value(0)).current;
  const dotScale2 = useRef(new Animated.Value(0)).current;
  const dotScale3 = useRef(new Animated.Value(0)).current;
  const pulseAnim = useRef(new Animated.Value(1)).current;
  const ringScale = useRef(new Animated.Value(0.5)).current;
  const ringOpacity = useRef(new Animated.Value(0)).current;

  useEffect(() => {
    // Logo entrance
    Animated.parallel([
      Animated.spring(logoScale, {
        toValue: 1,
        tension: 60,
        friction: 8,
        useNativeDriver: true,
      }),
      Animated.timing(logoOpacity, {
        toValue: 1,
        duration: 600,
        useNativeDriver: true,
      }),
    ]).start();

    // Pulse ring effect
    const pulseLoop = Animated.loop(
      Animated.sequence([
        Animated.parallel([
          Animated.timing(ringScale, {
            toValue: 1.6,
            duration: 1200,
            easing: Easing.out(Easing.ease),
            useNativeDriver: true,
          }),
          Animated.timing(ringOpacity, {
            toValue: 0,
            duration: 1200,
            easing: Easing.out(Easing.ease),
            useNativeDriver: true,
          }),
        ]),
        Animated.parallel([
          Animated.timing(ringScale, {
            toValue: 0.5,
            duration: 0,
            useNativeDriver: true,
          }),
          Animated.timing(ringOpacity, {
            toValue: 0.4,
            duration: 0,
            useNativeDriver: true,
          }),
        ]),
      ])
    );

    setTimeout(() => {
      ringOpacity.setValue(0.4);
      pulseLoop.start();
    }, 500);

    // Tagline entrance
    setTimeout(() => {
      Animated.parallel([
        Animated.timing(taglineOpacity, {
          toValue: 1,
          duration: 500,
          useNativeDriver: true,
        }),
        Animated.timing(taglineTranslateY, {
          toValue: 0,
          duration: 500,
          easing: Easing.out(Easing.ease),
          useNativeDriver: true,
        }),
      ]).start();
    }, 600);

    // Loading dots
    const animateDot = (dot: Animated.Value, delay: number) => {
      return Animated.loop(
        Animated.sequence([
          Animated.delay(delay),
          Animated.timing(dot, {
            toValue: 1,
            duration: 300,
            easing: Easing.out(Easing.ease),
            useNativeDriver: true,
          }),
          Animated.timing(dot, {
            toValue: 0.3,
            duration: 300,
            easing: Easing.in(Easing.ease),
            useNativeDriver: true,
          }),
        ])
      );
    };

    setTimeout(() => {
      dotScale1.setValue(0.3);
      dotScale2.setValue(0.3);
      dotScale3.setValue(0.3);
      animateDot(dotScale1, 0).start();
      animateDot(dotScale2, 200).start();
      animateDot(dotScale3, 400).start();
    }, 800);

    // Navigate after 2.5 seconds
    const timer = setTimeout(() => {
      Animated.timing(logoOpacity, {
        toValue: 0,
        duration: 300,
        useNativeDriver: true,
      }).start(() => {
        onFinish();
      });
    }, 2500);

    return () => clearTimeout(timer);
  }, []);

  return (
    <View style={styles.container}>
      <StatusBar barStyle="light-content" backgroundColor={Colors.primaryDark} />
      <LinearGradient
        colors={['#0F4C81', Colors.primaryBlue, '#2D8BC9']}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={StyleSheet.absoluteFill}
      />

      {/* Background decorative circles */}
      <View style={styles.decorCircle1} />
      <View style={styles.decorCircle2} />
      <View style={styles.decorCircle3} />

      {/* Center logo area */}
      <View style={styles.centerContent}>
        {/* Pulse ring */}
        <Animated.View
          style={[
            styles.pulseRing,
            {
              transform: [{ scale: ringScale }],
              opacity: ringOpacity,
            },
          ]}
        />

        {/* Logo */}
        <Animated.View
          style={[
            styles.logoContainer,
            {
              transform: [{ scale: logoScale }],
              opacity: logoOpacity,
            },
          ]}
        >
          <View style={styles.logoIcon}>
            <View style={styles.logoInner}>
              <Text style={styles.logoLetter}>H</Text>
            </View>
          </View>
        </Animated.View>

        {/* App Name */}
        <Animated.View style={{ opacity: logoOpacity }}>
          <Text style={styles.appName}>Hirenix</Text>
        </Animated.View>

        {/* Tagline */}
        <Animated.View
          style={{
            opacity: taglineOpacity,
            transform: [{ translateY: taglineTranslateY }],
          }}
        >
          <Text style={styles.tagline}>Kết nối cơ hội, xây dựng tương lai</Text>
        </Animated.View>
      </View>

      {/* Loading dots */}
      <View style={styles.loadingContainer}>
        <View style={styles.dotsRow}>
          {[dotScale1, dotScale2, dotScale3].map((dot, index) => (
            <Animated.View
              key={index}
              style={[
                styles.dot,
                {
                  opacity: dot,
                  transform: [{ scale: dot }],
                },
              ]}
            />
          ))}
        </View>
      </View>

      {/* Bottom version */}
      <Animated.Text style={[styles.version, { opacity: taglineOpacity }]}>
        Version 1.0.0
      </Animated.Text>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  decorCircle1: {
    position: 'absolute',
    width: 300,
    height: 300,
    borderRadius: 150,
    backgroundColor: 'rgba(255,255,255,0.04)',
    top: -80,
    right: -100,
  },
  decorCircle2: {
    position: 'absolute',
    width: 200,
    height: 200,
    borderRadius: 100,
    backgroundColor: 'rgba(255,255,255,0.03)',
    bottom: 100,
    left: -60,
  },
  decorCircle3: {
    position: 'absolute',
    width: 150,
    height: 150,
    borderRadius: 75,
    backgroundColor: 'rgba(255,255,255,0.05)',
    top: height * 0.3,
    left: width * 0.7,
  },
  centerContent: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  pulseRing: {
    position: 'absolute',
    width: 120,
    height: 120,
    borderRadius: 60,
    borderWidth: 2,
    borderColor: 'rgba(255,255,255,0.3)',
  },
  logoContainer: {
    marginBottom: Spacing.lg,
  },
  logoIcon: {
    width: 88,
    height: 88,
    borderRadius: 24,
    backgroundColor: 'rgba(255,255,255,0.15)',
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,0.25)',
  },
  logoInner: {
    width: 68,
    height: 68,
    borderRadius: 18,
    backgroundColor: Colors.white,
    alignItems: 'center',
    justifyContent: 'center',
  },
  logoLetter: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: 36,
    color: Colors.primaryBlue,
    letterSpacing: -1,
  },
  appName: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: 34,
    color: Colors.white,
    letterSpacing: 1,
    marginBottom: Spacing.sm,
  },
  tagline: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
    color: 'rgba(255,255,255,0.8)',
    textAlign: 'center',
    letterSpacing: 0.3,
  },
  loadingContainer: {
    position: 'absolute',
    bottom: 120,
  },
  dotsRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
  },
  dot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: 'rgba(255,255,255,0.9)',
  },
  version: {
    position: 'absolute',
    bottom: 50,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
    color: 'rgba(255,255,255,0.5)',
    letterSpacing: 0.5,
  },
});
