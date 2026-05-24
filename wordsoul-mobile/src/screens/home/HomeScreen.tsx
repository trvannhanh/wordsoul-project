import React, { useCallback, useEffect, useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  RefreshControl,
  Image,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { HomeStackParamList } from '../../navigation/MainTabs';
import { useAuth } from '../../contexts/AuthContext';
import { getUserProgress } from '../../services/user';
import { fetchTodayQuests } from '../../services/dailyQuest';
import type { UserProgressDto } from '../../types/UserDto';
import type { UserDailyQuestDto } from '../../types/DailyQuestDto';
import { ProgressBar } from '../../components/ui/ProgressBar';
import { Card } from '../../components/ui/Card';
import { Ionicons } from '@expo/vector-icons';
import { Button } from '../../components/ui/Button';
import { useNavigation } from '@react-navigation/native';
import type { BottomTabNavigationProp } from '@react-navigation/bottom-tabs';
import type { MainTabParamList } from '../../navigation/MainTabs';

type Props = NativeStackScreenProps<HomeStackParamList, 'HomeMain'>;

// XP thresholds theo level (mỗi level cần 100 XP)
const xpForLevel = (level: number) => level * 100;

export const HomeScreen: React.FC<Props> = ({ navigation }) => {
  const { user } = useAuth();
  const tabNav = useNavigation<BottomTabNavigationProp<MainTabParamList>>();

  const [progress, setProgress] = useState<UserProgressDto | null>(null);
  const [quests, setQuests] = useState<UserDailyQuestDto[]>([]);
  const [refreshing, setRefreshing] = useState(false);

  const loadData = useCallback(async () => {
    try {
      const [prog, q] = await Promise.all([
        getUserProgress(),
        fetchTodayQuests(),
      ]);
      setProgress(prog);
      setQuests(q.slice(0, 3));
    } catch {
      // silent fail
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const onRefresh = async () => {
    setRefreshing(true);
    await loadData();
    setRefreshing(false);
  };

  const xpPercent = user
    ? ((user.totalXP % xpForLevel(user.level)) / xpForLevel(user.level)) * 100
    : 0;

  return (
    <SafeAreaView className="flex-1 bg-gray-50 dark:bg-gray-950">
      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
      >
        {/* ── Header ── */}
        <View className="flex-row items-center justify-between px-5 pt-4 pb-3">
          <View>
            <Text className="text-gray-500 dark:text-gray-400 text-sm">
              Xin chào,
            </Text>
            <Text className="text-gray-900 dark:text-white text-xl font-bold">
              {user?.username} 👋
            </Text>
          </View>
          <TouchableOpacity onPress={() => navigation.navigate('Leaderboard')}>
            <View className="bg-white dark:bg-gray-800 rounded-full p-2.5 shadow">
              <Ionicons name="trophy-outline" size={22} color="#f59e0b" />
            </View>
          </TouchableOpacity>
        </View>

        {/* ── XP & Level Card ── */}
        <View className="mx-5 mb-4">
          <Card variant="elevated">
            <View className="flex-row items-center justify-between mb-3">
              <View className="flex-row items-center gap-x-3">
                {user?.avatarUrl ? (
                  <Image
                    source={{ uri: user.avatarUrl }}
                    className="w-12 h-12 rounded-full"
                  />
                ) : (
                  <View className="w-12 h-12 rounded-full bg-blue-100 dark:bg-blue-900 items-center justify-center">
                    <Text className="text-blue-600 dark:text-blue-300 text-lg font-bold">
                      {user?.username?.[0]?.toUpperCase()}
                    </Text>
                  </View>
                )}
                <View>
                  <Text className="text-gray-500 dark:text-gray-400 text-xs">
                    Cấp độ
                  </Text>
                  <Text className="text-gray-900 dark:text-white text-2xl font-black">
                    {user?.level}
                  </Text>
                </View>
              </View>

              {/* Stats row */}
              <View className="flex-row gap-x-4">
                <View className="items-center">
                  <Text className="text-orange-500 font-bold text-base">
                    🔥 {user?.streakDays}
                  </Text>
                  <Text className="text-gray-400 text-xs">Streak</Text>
                </View>
                <View className="items-center">
                  <Text className="text-violet-600 font-bold text-base">
                    ⚔️ {user?.totalAP}
                  </Text>
                  <Text className="text-gray-400 text-xs">AP</Text>
                </View>
              </View>
            </View>

            <ProgressBar
              value={xpPercent}
              color="#3b82f6"
              height={10}
              showLabel
              label={`${user?.totalXP ?? 0} XP`}
            />
          </Card>
        </View>

        {/* ── Review CTA ── */}
        {progress && progress.reviewWordCount > 0 && (
          <View className="mx-5 mb-4">
            <TouchableOpacity
              className="bg-blue-600 rounded-2xl p-4 flex-row items-center justify-between"
              activeOpacity={0.9}
              onPress={() => tabNav.navigate('LearnTab')}
            >
              <View>
                <Text className="text-white font-bold text-base">
                  📚 Ôn tập ngay!
                </Text>
                <Text className="text-blue-100 text-sm mt-0.5">
                  {progress.reviewWordCount} từ đang chờ bạn
                </Text>
              </View>
              <View className="bg-white/20 rounded-xl px-4 py-2">
                <Text className="text-white font-bold">Bắt đầu</Text>
              </View>
            </TouchableOpacity>
          </View>
        )}

        {/* ── Daily Quests ── */}
        <View className="mx-5 mb-4">
          <View className="flex-row items-center justify-between mb-3">
            <Text className="text-gray-900 dark:text-white text-base font-bold">
              Nhiệm vụ hôm nay
            </Text>
            <TouchableOpacity
              onPress={() => tabNav.navigate('ProfileTab')}
            >
              <Text className="text-blue-600 text-sm">Xem tất cả</Text>
            </TouchableOpacity>
          </View>

          <Card variant="outlined">
            {quests.length === 0 ? (
              <Text className="text-gray-400 text-center py-4">
                Chưa có nhiệm vụ hôm nay
              </Text>
            ) : (
              quests.map((q, idx) => (
                <View key={q.id}>
                  {idx > 0 && (
                    <View className="h-px bg-gray-100 dark:bg-gray-700 my-3" />
                  )}
                  <View className="flex-row items-center gap-x-3">
                    <View
                      className={`w-8 h-8 rounded-full items-center justify-center ${
                        q.isCompleted ? 'bg-green-100' : 'bg-blue-50 dark:bg-blue-900/30'
                      }`}
                    >
                      <Ionicons
                        name={q.isCompleted ? 'checkmark-circle' : 'ellipse-outline'}
                        size={18}
                        color={q.isCompleted ? '#22c55e' : '#3b82f6'}
                      />
                    </View>
                    <View className="flex-1">
                      <Text className="text-gray-800 dark:text-gray-200 text-sm font-semibold">
                        {q.title}
                      </Text>
                      <View className="mt-1">
                        <ProgressBar
                          value={(q.progress / q.targetValue) * 100}
                          color={q.isCompleted ? '#22c55e' : '#3b82f6'}
                          height={5}
                        />
                      </View>
                      <Text className="text-gray-400 text-xs mt-0.5">
                        {q.progress}/{q.targetValue}
                      </Text>
                    </View>
                    {q.isCompleted && !q.isClaimed && (
                      <View className="bg-amber-100 px-2 py-1 rounded-lg">
                        <Text className="text-amber-600 text-xs font-bold">
                          Nhận
                        </Text>
                      </View>
                    )}
                  </View>
                </View>
              ))
            )}
          </Card>
        </View>

        {/* ── Quick Actions ── */}
        <View className="mx-5 mb-6">
          <Text className="text-gray-900 dark:text-white text-base font-bold mb-3">
            Bắt đầu nhanh
          </Text>
          <View className="flex-row gap-x-3">
            {[
              {
                icon: '📖',
                label: 'Học từ mới',
                color: 'bg-blue-500',
                onPress: () => tabNav.navigate('LearnTab'),
              },
              {
                icon: '⚔️',
                label: 'Chiến đấu',
                color: 'bg-violet-500',
                onPress: () => tabNav.navigate('BattleTab'),
              },
              {
                icon: '🐾',
                label: 'Pet của tôi',
                color: 'bg-pink-500',
                onPress: () => tabNav.navigate('PetsTab'),
              },
            ].map((item) => (
              <TouchableOpacity
                key={item.label}
                className={`flex-1 ${item.color} rounded-2xl py-4 items-center`}
                activeOpacity={0.85}
                onPress={item.onPress}
              >
                <Text className="text-2xl mb-1">{item.icon}</Text>
                <Text className="text-white text-xs font-semibold">
                  {item.label}
                </Text>
              </TouchableOpacity>
            ))}
          </View>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
};
