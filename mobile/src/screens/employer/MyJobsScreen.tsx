import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  RefreshControl,
  StyleSheet,
  ActivityIndicator,
} from 'react-native';
import { employerJobApi } from '../../api/employerJobApi';
import { EmployerJobDto } from '../../types/employer';
import { useToast } from '../../contexts/ToastContext';
import { Colors } from '../../constants/theme';

type FilterType = 'All' | 'Active' | 'Closed';

export default function MyJobsScreen({ navigation }: any) {
  const [jobs, setJobs] = useState<EmployerJobDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [filter, setFilter] = useState<FilterType>('All');
  const { showToast } = useToast();

  const loadJobs = useCallback(async () => {
    try {
      const status = filter === 'All' ? undefined : filter;
      const data = await employerJobApi.getMyJobs(status);
      setJobs(data);
    } catch (error: any) {
      console.error('Failed to load jobs:', error);
      showToast(error.response?.data?.message || 'Failed to load jobs', 'error');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [filter, showToast]);

  useEffect(() => {
    loadJobs();
  }, [loadJobs]);

  // Refresh when screen comes into focus
  useEffect(() => {
    const unsubscribe = navigation.addListener('focus', () => {
      loadJobs();
    });
    return unsubscribe;
  }, [navigation, loadJobs]);

  const onRefresh = () => {
    setRefreshing(true);
    loadJobs();
  };

  const handleJobPress = (jobId: number) => {
    navigation.navigate('EmployerJobDetail', { jobId });
  };

  const handlePostJob = () => {
    navigation.navigate('PostJob');
  };

  const renderJobCard = ({ item }: { item: EmployerJobDto }) => (
    <TouchableOpacity
      style={styles.jobCard}
      onPress={() => handleJobPress(item.id)}
      activeOpacity={0.7}
    >
      {/* Header with Title and Status */}
      <View style={styles.jobHeader}>
        <Text style={styles.jobTitle} numberOfLines={2}>
          {item.title}
        </Text>
        <View
          style={[
            styles.statusBadge,
            item.status === 'Active' ? styles.activeBadge : styles.closedBadge,
          ]}
        >
          <Text
            style={[
              styles.statusText,
              item.status === 'Active' ? styles.activeText : styles.closedText,
            ]}
          >
            {item.status}
          </Text>
        </View>
      </View>

      {/* Job Info */}
      <Text style={styles.jobInfo} numberOfLines={1}>
        {item.employmentType} • {item.workMode} • {item.locationName}
      </Text>

      {/* Stats Row */}
      <View style={styles.statsRow}>
        <View style={styles.statItem}>
          <Text style={styles.statIcon}>👁️</Text>
          <Text style={styles.statText}>{item.viewCount} views</Text>
        </View>
        <View style={styles.statItem}>
          <Text style={styles.statIcon}>📝</Text>
          <Text style={styles.statText}>
            {item.applicationCount} application{item.applicationCount !== 1 ? 's' : ''}
          </Text>
        </View>
      </View>

      {/* Footer with Date and Expiry */}
      <View style={styles.jobFooter}>
        <Text style={styles.dateText}>
          Posted: {new Date(item.createdAt).toLocaleDateString()}
        </Text>
        <Text style={styles.expiryText}>
          Expires: {new Date(item.expiryDate).toLocaleDateString()}
        </Text>
      </View>
    </TouchableOpacity>
  );

  const renderEmptyState = () => (
    <View style={styles.emptyContainer}>
      <Text style={styles.emptyIcon}>📋</Text>
      <Text style={styles.emptyTitle}>No Jobs Posted Yet</Text>
      <Text style={styles.emptyText}>
        Start attracting top talent by posting your first job opening
      </Text>
      <TouchableOpacity style={styles.emptyButton} onPress={handlePostJob}>
        <Text style={styles.emptyButtonText}>Post Your First Job</Text>
      </TouchableOpacity>
    </View>
  );

  const renderHeader = () => (
    <View style={styles.header}>
      <Text style={styles.headerTitle}>My Posted Jobs</Text>
      <Text style={styles.headerSubtitle}>
        {jobs.length} job{jobs.length !== 1 ? 's' : ''} total
      </Text>
    </View>
  );

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color={Colors.primaryBlue} />
        <Text style={styles.loadingText}>Loading jobs...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      {/* Filter Tabs */}
      <View style={styles.filterContainer}>
        {(['All', 'Active', 'Closed'] as FilterType[]).map((tab) => (
          <TouchableOpacity
            key={tab}
            style={[styles.filterTab, filter === tab && styles.activeFilterTab]}
            onPress={() => setFilter(tab)}
            activeOpacity={0.7}
          >
            <Text
              style={[
                styles.filterText,
                filter === tab && styles.activeFilterText,
              ]}
            >
              {tab}
            </Text>
          </TouchableOpacity>
        ))}
      </View>

      {/* Jobs List */}
      <FlatList
        data={jobs}
        renderItem={renderJobCard}
        keyExtractor={(item) => item.id.toString()}
        contentContainerStyle={[
          styles.listContent,
          jobs.length === 0 && styles.emptyListContent,
        ]}
        ListHeaderComponent={jobs.length > 0 ? renderHeader : null}
        ListEmptyComponent={renderEmptyState}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={onRefresh}
            colors={[Colors.primaryBlue]}
            tintColor={Colors.primaryBlue}
          />
        }
        showsVerticalScrollIndicator={false}
      />

      {/* Floating Action Button */}
      {jobs.length > 0 && (
        <TouchableOpacity
          style={styles.fab}
          onPress={handlePostJob}
          activeOpacity={0.8}
        >
          <Text style={styles.fabText}>+</Text>
        </TouchableOpacity>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.gray50,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: Colors.gray50,
  },
  loadingText: {
    marginTop: 12,
    fontSize: 16,
    color: Colors.gray600,
  },
  filterContainer: {
    flexDirection: 'row',
    paddingHorizontal: 16,
    paddingVertical: 12,
    backgroundColor: '#FFFFFF',
    borderBottomWidth: 1,
    borderBottomColor: Colors.gray200,
  },
  filterTab: {
    flex: 1,
    paddingVertical: 8,
    paddingHorizontal: 16,
    marginHorizontal: 4,
    borderRadius: 8,
    backgroundColor: Colors.gray50,
    alignItems: 'center',
  },
  activeFilterTab: {
    backgroundColor: Colors.primaryBlue,
  },
  filterText: {
    fontSize: 14,
    fontWeight: '600',
    color: Colors.gray600,
  },
  activeFilterText: {
    color: '#FFFFFF',
  },
  listContent: {
    padding: 16,
  },
  emptyListContent: {
    flexGrow: 1,
  },
  header: {
    marginBottom: 16,
  },
  headerTitle: {
    fontSize: 24,
    fontWeight: 'bold',
    color: Colors.gray900,
    marginBottom: 4,
  },
  headerSubtitle: {
    fontSize: 14,
    color: Colors.gray600,
  },
  jobCard: {
    backgroundColor: '#FFFFFF',
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: Colors.gray200,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.05,
    shadowRadius: 4,
    elevation: 2,
  },
  jobHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 8,
  },
  jobTitle: {
    flex: 1,
    fontSize: 18,
    fontWeight: '600',
    color: Colors.gray900,
    marginRight: 12,
  },
  statusBadge: {
    paddingHorizontal: 12,
    paddingVertical: 4,
    borderRadius: 12,
  },
  activeBadge: {
    backgroundColor: '#E8F5E9',
  },
  closedBadge: {
    backgroundColor: '#FFEBEE',
  },
  statusText: {
    fontSize: 12,
    fontWeight: '600',
  },
  activeText: {
    color: '#2E7D32',
  },
  closedText: {
    color: '#C62828',
  },
  jobInfo: {
    fontSize: 14,
    color: Colors.gray600,
    marginBottom: 12,
  },
  statsRow: {
    flexDirection: 'row',
    marginBottom: 12,
  },
  statItem: {
    flexDirection: 'row',
    alignItems: 'center',
    marginRight: 20,
  },
  statIcon: {
    fontSize: 16,
    marginRight: 6,
  },
  statText: {
    fontSize: 14,
    color: Colors.gray600,
  },
  jobFooter: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingTop: 12,
    borderTopWidth: 1,
    borderTopColor: Colors.gray200,
  },
  dateText: {
    fontSize: 12,
    color: Colors.gray600,
  },
  expiryText: {
    fontSize: 12,
    color: Colors.gray600,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 32,
  },
  emptyIcon: {
    fontSize: 64,
    marginBottom: 16,
  },
  emptyTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: Colors.gray900,
    marginBottom: 8,
    textAlign: 'center',
  },
  emptyText: {
    fontSize: 14,
    color: Colors.gray600,
    textAlign: 'center',
    marginBottom: 24,
    lineHeight: 20,
  },
  emptyButton: {
    backgroundColor: Colors.primaryBlue,
    paddingHorizontal: 24,
    paddingVertical: 12,
    borderRadius: 8,
  },
  emptyButtonText: {
    color: '#FFFFFF',
    fontSize: 16,
    fontWeight: '600',
  },
  fab: {
    position: 'absolute',
    right: 20,
    bottom: 20,
    width: 56,
    height: 56,
    borderRadius: 28,
    backgroundColor: Colors.primaryBlue,
    justifyContent: 'center',
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 8,
  },
  fabText: {
    fontSize: 32,
    color: '#FFFFFF',
    fontWeight: '300',
  },
});
