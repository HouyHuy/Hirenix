/**
 * AuthNavigator — Hirenix
 * Navigation stack cho luồng Auth: Splash → Onboarding → Welcome → Login/Register
 */
import React, { useState } from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { SplashScreen } from '../screens/auth/SplashScreen';
import { OnboardingScreen } from '../screens/auth/OnboardingScreen';
import { WelcomeScreen } from '../screens/auth/WelcomeScreen';
import { LoginScreen } from '../screens/auth/LoginScreen';
import { RegisterScreen } from '../screens/auth/RegisterScreen';
import { OtpVerificationScreen } from '../screens/auth/OtpVerificationScreen';

export type AuthStackParamList = {
  Splash: undefined;
  Onboarding: undefined;
  Welcome: undefined;
  Login: undefined;
  Register: undefined;
  OtpVerification: { email: string };
};

const Stack = createNativeStackNavigator<AuthStackParamList>();

export const AuthNavigator: React.FC = () => {
  const [showSplash, setShowSplash] = useState(true);

  if (showSplash) {
    return <SplashScreen onFinish={() => setShowSplash(false)} />;
  }

  return (
    <Stack.Navigator
      initialRouteName="Onboarding"
      screenOptions={{
        headerShown: false,
        animation: 'slide_from_right',
        animationDuration: 300,
      }}
    >
      <Stack.Screen name="Onboarding">
        {({ navigation }) => (
          <OnboardingScreen
            onFinish={() => navigation.replace('Welcome')}
          />
        )}
      </Stack.Screen>

      <Stack.Screen name="Welcome">
        {({ navigation }) => (
          <WelcomeScreen
            onLogin={() => navigation.navigate('Login')}
            onRegister={() => navigation.navigate('Register')}
          />
        )}
      </Stack.Screen>

      <Stack.Screen name="Login">
        {({ navigation }) => (
          <LoginScreen
            onBack={() => navigation.goBack()}
            onLogin={() => {
              // TODO: Navigate to main app after auth
              console.log('Login success');
            }}
            onForgotPassword={() => {
              // TODO: Navigate to ForgotPassword
              console.log('Forgot password');
            }}
            onRegister={() => navigation.navigate('Register')}
          />
        )}
      </Stack.Screen>

      <Stack.Screen name="Register">
        {({ navigation, route }) => (
          <RegisterScreen
            onBack={() => navigation.goBack()}
            onRegister={(email: string) => {
              navigation.navigate('OtpVerification', { email });
            }}
            onLogin={() => navigation.navigate('Login')}
          />
        )}
      </Stack.Screen>

      <Stack.Screen name="OtpVerification">
        {({ navigation, route }) => (
          <OtpVerificationScreen
            email={(route.params as any)?.email || ''}
            onBack={() => navigation.goBack()}
            onVerified={() => {
              // TODO: Navigate to main app after verification
              console.log('OTP verified, navigate to main app');
            }}
          />
        )}
      </Stack.Screen>
    </Stack.Navigator>
  );
};
