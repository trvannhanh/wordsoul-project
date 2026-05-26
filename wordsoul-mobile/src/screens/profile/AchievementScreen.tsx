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
import { fetchMyAchievements, claimAchievement } from '../../services/achievement';
import type { UserAchievementDto } from '../../types/AchievementDto';

export const AchievementScreen: React.FC = () => {
  const [achievements, setAchievements] = useState<UserAchievementDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [claiming, setClaiming] = useState<number | null>(null);


  const load = async () => {
    try {
      const data = await fetchMyAchievements();
      setAchievements(data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleClaim = async (id: number) => {
    setClaiming(id);
    try {
      await claimAchievement(id); // id is achievementId
      await load();
      Alert.alert('🎉 Nhận thưởng!', 'Bạn đã nhận phần thưởng thành tích.');
    } catch {
      Alert.alert('Lỗi', 'Không thể nhận thưởng');
    } finally {
      setClaiming(null);
    }
  };

  const AchievementCard = ({ item }: { item: UserAchievementDto }) => {
    const isCompleted = item.isCompleted;
    const canClaim = isCompleted;

    return (
      <View
        className={`flex-row items-center bg-white dark:bg-gray-800 rounded-2xl p-4 mb-3 border-2 ${
          isCompleted ? 'border-yellow-300' : 'border-transparent opacity-60'
        }`}
      >
        <View className="w-12 h-12 rounded-full bg-yellow-100 dark:bg-yellow-900/30 items-center justify-center mr-4">
          <Text className="text-2xl">🏅</Text>
        </View>
        <View className="flex-1">
          <Text className="text-gray-900 dark:text-white font-bold">{item.name}</Text>
          <Text className="text-gray-500 dark:text-gray-400 text-sm mt-0.5">
            {item.description}
          </Text>
          {!isCompleted && (
            <Text className="text-gray-400 text-xs mt-1">
              {item.progressValue}/{item.targetValue}
            </Text>
          )}
        </View>
        {canClaim && (
          <TouchableOpacity
            className="bg-yellow-400 px-3 py-1.5 rounded-xl ml-3"
            onPress={() => handleClaim(item.achievementId)}
            disabled={claiming === item.achievementId}
          >
            {claiming === item.achievementId ? (
              <ActivityIndicator size="small" color="white" />
            ) : (
              <Text className="text-white font-bold text-xs">Nhận</Text>
            )}
          </TouchableOpacity>
        )}
      </View>
    );
  };

  return (
    <SafeAreaView className="flex-1 bg-gray-50 dark:bg-gray-950">
      <Text className="text-gray-900 dark:text-white text-xl font-black px-4 py-3">
        🏆 Thành tích
      </Text>

      {loading ? (
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator size="large" color="#3b82f6" />
        </View>
      ) : (
        <FlatList
          data={achievements}
          keyExtractor={(item) => String(item.achievementId)}
          renderItem={({ item }) => <AchievementCard item={item} />}
          contentContainerStyle={{ padding: 16, paddingBottom: 32 }}
          showsVerticalScrollIndicator={false}
        />
      )}
    </SafeAreaView>
  );
};
