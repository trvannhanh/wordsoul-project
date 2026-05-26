import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { fetchTodayQuests, claimQuestReward } from '../../services/dailyQuest';
import type { UserDailyQuestDto } from '../../types/DailyQuestDto';
import { ProgressBar } from '../../components/ui/ProgressBar';

export const DailyQuestScreen: React.FC = () => {
  const [quests, setQuests] = useState<UserDailyQuestDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [claiming, setClaiming] = useState<number | null>(null);

  const load = async () => {
    try {
      const data = await fetchTodayQuests();
      setQuests(data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleClaim = async (id: number) => {
    setClaiming(id);
    try {
      const res = await claimQuestReward(id);
      await load();
      Alert.alert('🎁 Phần thưởng!', `+${res.rewardValue} ${res.rewardType}`);
    } catch {
      Alert.alert('Lỗi', 'Không thể nhận thưởng');
    } finally {
      setClaiming(null);
    }
  };

  const QuestCard = ({ item }: { item: UserDailyQuestDto }) => {
    const progressPct = Math.min((item.progress / item.targetValue) * 100, 100);
    const canClaim = item.isCompleted && !item.isClaimed;

    return (
      <View
        className={`bg-white dark:bg-gray-800 rounded-2xl p-4 mb-3 border-2 ${
          item.isClaimed
            ? 'border-green-200 opacity-60'
            : item.isCompleted
            ? 'border-blue-300'
            : 'border-transparent'
        }`}
      >
        <View className="flex-row items-start justify-between mb-2">
          <View className="flex-1 mr-3">
            <Text className="text-gray-900 dark:text-white font-bold">{item.title}</Text>
            <Text className="text-gray-500 dark:text-gray-400 text-sm mt-0.5">
              {item.description}
            </Text>
          </View>
          <View className="items-end">
            <Text className="text-yellow-500 font-bold text-sm">+{item.rewardValue}</Text>
            <Text className="text-gray-400 text-xs">{item.rewardType}</Text>
          </View>
        </View>

        <View className="flex-row items-center gap-x-3">
          <View className="flex-1">
            <ProgressBar value={progressPct} color={item.isCompleted ? '#22c55e' : '#3b82f6'} height={6} animated={false} />
          </View>
          <Text className="text-gray-500 text-xs w-16 text-right">
            {item.progress}/{item.targetValue}
          </Text>
        </View>

        {canClaim && (
          <TouchableOpacity
            className="mt-3 bg-blue-600 rounded-xl py-2 items-center"
            onPress={() => handleClaim(item.id)}
            disabled={claiming === item.id}
          >
            {claiming === item.id ? (
              <ActivityIndicator size="small" color="white" />
            ) : (
              <Text className="text-white font-bold text-sm">Nhận thưởng 🎁</Text>
            )}
          </TouchableOpacity>
        )}
        {item.isClaimed && (
          <Text className="text-green-500 font-bold text-center mt-2 text-sm">✓ Đã nhận</Text>
        )}
      </View>
    );
  };

  return (
    <SafeAreaView className="flex-1 bg-gray-50 dark:bg-gray-950">
      <Text className="text-gray-900 dark:text-white text-xl font-black px-4 py-3">
        📋 Nhiệm vụ hôm nay
      </Text>

      {loading ? (
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator size="large" color="#3b82f6" />
        </View>
      ) : (
        <FlatList
          data={quests}
          keyExtractor={(item) => String(item.id)}
          renderItem={({ item }) => <QuestCard item={item} />}
          contentContainerStyle={{ padding: 16, paddingBottom: 32 }}
          showsVerticalScrollIndicator={false}
          ListEmptyComponent={
            <View className="items-center py-16">
              <Text className="text-4xl mb-3">🌟</Text>
              <Text className="text-gray-500 text-base">Không có nhiệm vụ hôm nay</Text>
            </View>
          }
        />
      )}
    </SafeAreaView>
  );
};
