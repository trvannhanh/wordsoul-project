import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  Image,
  RefreshControl,
  ActivityIndicator,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { ProfileStackParamList } from '../../navigation/MainTabs';
import { useAuth } from '../../contexts/AuthContext';
import { getUserProgress } from '../../services/user';
import type { UserProgressDto } from '../../types/UserDto';
import { ProgressBar } from '../../components/ui/ProgressBar';
import { Ionicons } from '@expo/vector-icons';

type Props = NativeStackScreenProps<ProfileStackParamList, 'ProfileMain'>;

const XP_PER_LEVEL = 100;

export const ProfileScreen: React.FC<Props> = ({ navigation }) => {
  const { user, logout } = useAuth();
  const [progress, setProgress] = useState<UserProgressDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const load = async () => {
    try {
      const data = await getUserProgress();
      setProgress(data);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => { load(); }, []);

  const xpInLevel = (user?.totalXP ?? 0) % XP_PER_LEVEL;
  const xpPercent = (xpInLevel / XP_PER_LEVEL) * 100;

  const StatCard = ({ icon, label, value }: { icon: string; label: string; value: string | number }) => (
    <View className="flex-1 bg-gray-50 dark:bg-gray-800 rounded-2xl p-4 items-center">
      <Text className="text-2xl mb-1">{icon}</Text>
      <Text className="text-gray-900 dark:text-white font-black text-xl">{value}</Text>
      <Text className="text-gray-500 text-xs text-center">{label}</Text>
    </View>
  );

  const MenuItem = ({
    icon,
    label,
    onPress,
    danger,
  }: {
    icon: string;
    label: string;
    onPress: () => void;
    danger?: boolean;
  }) => (
    <TouchableOpacity
      className="flex-row items-center bg-white dark:bg-gray-800 px-4 py-4 border-b border-gray-100 dark:border-gray-700"
      onPress={onPress}
      activeOpacity={0.7}
    >
      <Text className="text-xl mr-4">{icon}</Text>
      <Text
        className={`flex-1 text-base font-medium ${
          danger ? 'text-red-500' : 'text-gray-800 dark:text-gray-200'
        }`}
      >
        {label}
      </Text>
      <Ionicons name="chevron-forward" size={18} color="#9ca3af" />
    </TouchableOpacity>
  );

  return (
    <SafeAreaView className="flex-1 bg-gray-50 dark:bg-gray-950">
      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={() => { setRefreshing(true); load(); }} />
        }
        contentContainerStyle={{ paddingBottom: 32 }}
      >
        {/* Profile header */}
        <View className="bg-white dark:bg-gray-900 px-6 pt-6 pb-8 items-center mb-4">
          {/* Avatar */}
          <View className="w-24 h-24 rounded-full bg-blue-100 dark:bg-blue-900 items-center justify-center mb-3 overflow-hidden">
            {user?.avatarUrl ? (
              <Image source={{ uri: user.avatarUrl }} className="w-full h-full" resizeMode="cover" />
            ) : (
              <Text className="text-4xl">👤</Text>
            )}
          </View>

          <Text className="text-2xl font-black text-gray-900 dark:text-white">
            {user?.username}
          </Text>
          <Text className="text-gray-500 dark:text-gray-400 text-sm mt-0.5">
            {user?.email}
          </Text>

          {/* Level + XP */}
          <View className="mt-4 w-full max-w-xs">
            <View className="flex-row justify-between mb-1">
              <Text className="text-gray-700 dark:text-gray-300 font-bold text-sm">
                Cấp {user?.level}
              </Text>
              <Text className="text-gray-500 text-sm">
                {xpInLevel}/{XP_PER_LEVEL} XP
              </Text>
            </View>
            <ProgressBar value={xpPercent} color="#3b82f6" height={10} />
          </View>
        </View>

        {/* Stats */}
        <View className="px-4 mb-4">
          <Text className="text-gray-900 dark:text-white font-bold text-sm mb-3">
            Thống kê
          </Text>
          {loading ? (
            <ActivityIndicator size="small" color="#3b82f6" />
          ) : (
            <View className="flex-row gap-x-3 mb-3">
              <StatCard icon="🔥" label="Streak" value={`${user?.streakDays ?? 0} ngày`} />
              <StatCard icon="⚡" label="Tổng XP" value={user?.totalXP ?? 0} />
              <StatCard icon="💎" label="AP" value={user?.totalAP ?? 0} />
            </View>
          )}
          {progress && (
            <View className="flex-row gap-x-3">
              <StatCard icon="📚" label="Ôn tập" value={progress.reviewWordCount} />
              <StatCard icon="⭐" label="Chủ đề" value={progress.themePreferences?.length ?? 0} />
              <StatCard icon="🎯" label="Đề xuất" value={progress.recommendedSets?.length ?? 0} />
            </View>
          )}
        </View>

        {/* Menu */}
        <View className="rounded-2xl overflow-hidden mx-4 mb-3">
          <MenuItem
            icon="🏆"
            label="Thành tích"
            onPress={() => navigation.navigate('Achievements')}
          />
          <MenuItem
            icon="📋"
            label="Nhiệm vụ hằng ngày"
            onPress={() => navigation.navigate('DailyQuests')}
          />
          <MenuItem
            icon="⚙️"
            label="Cài đặt"
            onPress={() => navigation.navigate('Settings')}
          />
        </View>

        <View className="rounded-2xl overflow-hidden mx-4">
          <MenuItem
            icon="🚪"
            label="Đăng xuất"
            onPress={logout}
            danger
          />
        </View>
      </ScrollView>
    </SafeAreaView>
  );
};
