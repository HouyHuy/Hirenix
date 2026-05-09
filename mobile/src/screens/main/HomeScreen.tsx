/**
 * HomeScreen — Hirenix
 * Trang chủ hiển thị danh sách công việc và tìm kiếm
 */
import React from 'react';
import {
  View, Text, StyleSheet, ScrollView, TouchableOpacity,
  StatusBar, TextInput as RNTextInput,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Colors, Typography, Spacing, BorderRadius } from '../../constants/theme';
import { useAuth } from '../../contexts/AuthContext';

export const HomeScreen: React.FC = () => {
  const insets = useSafeAreaInsets();
  const { user } = useAuth();

  return (
    <View style={styles.container}>
      <StatusBar barStyle="dark-content" backgroundColor={Colors.white} />
      
      {/* Header */}
      <View style={[styles.header, { paddingTop: insets.top + Spacing.md }]}>
        <View>
          <Text style={styles.greeting}>Xin chào 👋</Text>
          <Text style={styles.userName}>{user?.email?.split('@')[0] || 'User'}</Text>
        </View>
        <TouchableOpacity style={styles.notificationBtn}>
          <Ionicons name="notifications-outline" size={24} color={Colors.gray800} />
          <View style={styles.badge} />
        </TouchableOpacity>
      </View>

      {/* Search Bar */}
      <View style={styles.searchSection}>
        <View style={styles.searchBar}>
          <Ionicons name="search-outline" size={20} color={Colors.gray400} />
          <RNTextInput
            style={styles.searchInput}
            placeholder="Tìm kiếm công việc, công ty..."
            placeholderTextColor={Colors.gray400}
          />
        </View>
        <TouchableOpacity style={styles.filterBtn}>
          <Ionicons name="options-outline" size={20} color={Colors.white} />
        </TouchableOpacity>
      </View>

      <ScrollView
        style={styles.content}
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.scrollContent}
      >
        {/* Categories */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Danh mục phổ biến</Text>
          <ScrollView
            horizontal
            showsHorizontalScrollIndicator={false}
            contentContainerStyle={styles.categoriesRow}
          >
            {CATEGORIES.map((cat) => (
              <TouchableOpacity key={cat.id} style={styles.categoryCard}>
                <View style={[styles.categoryIcon, { backgroundColor: cat.color }]}>
                  <Ionicons name={cat.icon as any} size={24} color={Colors.white} />
                </View>
                <Text style={styles.categoryName}>{cat.name}</Text>
                <Text style={styles.categoryCount}>{cat.count} việc</Text>
              </TouchableOpacity>
            ))}
          </ScrollView>
        </View>

        {/* Featured Jobs */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Việc làm nổi bật</Text>
            <TouchableOpacity>
              <Text style={styles.seeAllText}>Xem tất cả</Text>
            </TouchableOpacity>
          </View>
          
          {FEATURED_JOBS.map((job) => (
            <TouchableOpacity key={job.id} style={styles.jobCard}>
              <View style={styles.jobHeader}>
                <View style={styles.companyLogo}>
                  <Text style={styles.companyLogoText}>{job.company[0]}</Text>
                </View>
                <View style={styles.jobInfo}>
                  <Text style={styles.jobTitle}>{job.title}</Text>
                  <Text style={styles.companyName}>{job.company}</Text>
                </View>
                <TouchableOpacity style={styles.bookmarkBtn}>
                  <Ionicons name="bookmark-outline" size={20} color={Colors.gray600} />
                </TouchableOpacity>
              </View>
              
              <View style={styles.jobDetails}>
                <View style={styles.jobTag}>
                  <Ionicons name="location-outline" size={14} color={Colors.gray600} />
                  <Text style={styles.jobTagText}>{job.location}</Text>
                </View>
                <View style={styles.jobTag}>
                  <Ionicons name="time-outline" size={14} color={Colors.gray600} />
                  <Text style={styles.jobTagText}>{job.type}</Text>
                </View>
                <View style={styles.jobTag}>
                  <Ionicons name="cash-outline" size={14} color={Colors.gray600} />
                  <Text style={styles.jobTagText}>{job.salary}</Text>
                </View>
              </View>

              <View style={styles.jobFooter}>
                <Text style={styles.postedTime}>{job.posted}</Text>
                <View style={styles.applicants}>
                  <Ionicons name="people-outline" size={14} color={Colors.primaryBlue} />
                  <Text style={styles.applicantsText}>{job.applicants} ứng viên</Text>
                </View>
              </View>
            </TouchableOpacity>
          ))}
        </View>
      </ScrollView>
    </View>
  );
};

const CATEGORIES = [
  { id: 1, name: 'IT & Software', icon: 'code-slash', color: Colors.primaryBlue, count: 234 },
  { id: 2, name: 'Marketing', icon: 'megaphone', color: Colors.accentTeal, count: 156 },
  { id: 3, name: 'Design', icon: 'color-palette', color: Colors.warningAmber, count: 89 },
  { id: 4, name: 'Sales', icon: 'trending-up', color: Colors.successText, count: 178 },
];

const FEATURED_JOBS = [
  {
    id: 1,
    title: 'Senior React Native Developer',
    company: 'TechCorp Vietnam',
    location: 'Hà Nội',
    type: 'Full-time',
    salary: '25-35 triệu',
    posted: '2 giờ trước',
    applicants: 12,
  },
  {
    id: 2,
    title: 'UI/UX Designer',
    company: 'Creative Studio',
    location: 'TP.HCM',
    type: 'Remote',
    salary: '15-25 triệu',
    posted: '5 giờ trước',
    applicants: 8,
  },
  {
    id: 3,
    title: 'Backend Developer (Node.js)',
    company: 'StartupXYZ',
    location: 'Đà Nẵng',
    type: 'Full-time',
    salary: '20-30 triệu',
    posted: '1 ngày trước',
    applicants: 15,
  },
];

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.gray50 },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: Spacing.base,
    paddingBottom: Spacing.md,
    backgroundColor: Colors.white,
  },
  greeting: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
    color: Colors.gray600,
  },
  userName: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingMd,
    color: Colors.gray900,
    marginTop: 2,
  },
  notificationBtn: {
    width: 44,
    height: 44,
    borderRadius: BorderRadius.md,
    backgroundColor: Colors.gray100,
    alignItems: 'center',
    justifyContent: 'center',
    position: 'relative',
  },
  badge: {
    position: 'absolute',
    top: 10,
    right: 10,
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: Colors.dangerRed,
  },
  searchSection: {
    flexDirection: 'row',
    paddingHorizontal: Spacing.base,
    paddingVertical: Spacing.md,
    gap: Spacing.sm,
    backgroundColor: Colors.white,
    borderBottomWidth: 1,
    borderBottomColor: Colors.gray100,
  },
  searchBar: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: Colors.gray50,
    borderRadius: BorderRadius.lg,
    paddingHorizontal: Spacing.md,
    gap: Spacing.sm,
    height: 48,
  },
  searchInput: {
    flex: 1,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
    color: Colors.gray900,
  },
  filterBtn: {
    width: 48,
    height: 48,
    borderRadius: BorderRadius.lg,
    backgroundColor: Colors.primaryBlue,
    alignItems: 'center',
    justifyContent: 'center',
  },
  content: { flex: 1 },
  scrollContent: { paddingBottom: Spacing['2xl'] },
  section: { marginTop: Spacing.lg },
  sectionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: Spacing.base,
    marginBottom: Spacing.md,
  },
  sectionTitle: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingSm,
    color: Colors.gray900,
    paddingHorizontal: Spacing.base,
  },
  seeAllText: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodySm,
    color: Colors.primaryBlue,
  },
  categoriesRow: {
    paddingHorizontal: Spacing.base,
    gap: Spacing.md,
  },
  categoryCard: {
    width: 120,
    padding: Spacing.md,
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.lg,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: Colors.gray100,
  },
  categoryIcon: {
    width: 48,
    height: 48,
    borderRadius: BorderRadius.md,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: Spacing.sm,
  },
  categoryName: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodySm,
    color: Colors.gray900,
    textAlign: 'center',
    marginBottom: 2,
  },
  categoryCount: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
    color: Colors.gray600,
  },
  jobCard: {
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.lg,
    padding: Spacing.base,
    marginHorizontal: Spacing.base,
    marginBottom: Spacing.md,
    borderWidth: 1,
    borderColor: Colors.gray100,
  },
  jobHeader: {
    flexDirection: 'row',
    marginBottom: Spacing.md,
  },
  companyLogo: {
    width: 48,
    height: 48,
    borderRadius: BorderRadius.md,
    backgroundColor: Colors.primaryBlue,
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: Spacing.sm,
  },
  companyLogoText: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingSm,
    color: Colors.white,
  },
  jobInfo: { flex: 1 },
  jobTitle: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.bodyLg,
    color: Colors.gray900,
    marginBottom: 2,
  },
  companyName: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodySm,
    color: Colors.gray600,
  },
  bookmarkBtn: {
    width: 32,
    height: 32,
    alignItems: 'center',
    justifyContent: 'center',
  },
  jobDetails: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: Spacing.sm,
    marginBottom: Spacing.md,
  },
  jobTag: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    paddingVertical: 4,
    paddingHorizontal: Spacing.sm,
    backgroundColor: Colors.gray50,
    borderRadius: BorderRadius.sm,
  },
  jobTagText: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
    color: Colors.gray600,
  },
  jobFooter: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingTop: Spacing.sm,
    borderTopWidth: 1,
    borderTopColor: Colors.gray100,
  },
  postedTime: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
    color: Colors.gray600,
  },
  applicants: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  applicantsText: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.caption,
    color: Colors.primaryBlue,
  },
});
