/**
 * AdminNavigator — Hirenix
 * Stack navigation cho Admin Panel (tách biệt với MainNavigator)
 */
import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { TouchableOpacity, View, Text, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { AdminDashboardScreen } from '../screens/admin/AdminDashboardScreen';
import { useAuth } from '../contexts/AuthContext';
import { Colors, Typography, Spacing } from '../constants/theme';

export type AdminStackParamList = {
  AdminDashboard: undefined;
  // Future admin screens:
  // AdminUsers: undefined;
  // AdminJobs: undefined;
  // AdminCompanies: undefined;
  // AdminReports: undefined;
};

const Stack = createNativeStackNavigator<AdminStackParamList>();

export const AdminNavigator: React.FC = () => {
  const { logout, user } = useAuth();

  const handleLogout = async () => {
    try {
      await logout();
    } catch (error) {
      console.error('Logout error:', error);
    }
  };

  return (
    <Stack.Navigator
      screenOptions={{
        headerShown: true,
        headerStyle: {
          backgroundColor: Colors.primaryDark,
        },
        headerTintColor: Colors.white,
        headerTitleStyle: {
          fontFamily: Typography.fontFamily.bold,
          fontSize: Typography.size.bodyLg,
        },
        animation: 'slide_from_right',
      }}
    >
      <Stack.Screen
        name="AdminDashboard"
        component={AdminDashboardScreen}
        options={{
          headerShown: false, // Dashboard có header riêng
        }}
      />
      
      {/* Future admin screens can be added here:
      <Stack.Screen
        name="AdminUsers"
        component={AdminUsersScreen}
        options={{ title: 'User Management' }}
      />
      <Stack.Screen
        name="AdminJobs"
        component={AdminJobsScreen}
        options={{ title: 'Job Review' }}
      />
      */}
    </Stack.Navigator>
  );
};

const styles = StyleSheet.create({
  headerRight: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.md,
    marginRight: Spacing.sm,
  },
  userInfo: {
    alignItems: 'flex-end',
  },
  userEmail: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.caption,
    color: Colors.gray100,
  },
  userRole: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.overline,
    color: Colors.accentTeal,
    textTransform: 'uppercase',
  },
  logoutButton: {
    padding: Spacing.sm,
  },
});
