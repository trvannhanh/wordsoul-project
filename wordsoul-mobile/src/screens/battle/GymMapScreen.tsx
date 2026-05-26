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
import type { BattleStackParamList } from '../../navigation/MainTabs';
import { fetchGyms } from '../../services/gym';
import type { GymLeaderDto } from '../../types/GymTypes';
import { GymStatus } from '../../types/GymTypes';
import { ProgressBar } from '../../components/ui/ProgressBar';
import { Ionicons } from '@expo/vector-icons';

type Props = NativeStackScreenProps<BattleStackParamList, 'GymMap'>;

const statusConfig = {
  [GymStatus.Locked]: {
    label: 'Khóa',
    icon: '🔒',
    style: 'bg-gray-100 dark:bg-gray-800 opacity-70',
    textStyle: 'text-gray-400',
  },
  [GymStatus.Unlocked]: {
    label: 'Sẵn sàng',
    icon: '⚔️',
    style: 'bg-white dark:bg-gray-800 border-2 border-blue-400',
    textStyle: 'text-blue-600',
  },
  [GymStatus.Defeated]: {
    label: 'Hoàn thành',
    icon: '🏆',
    style: 'bg-white dark:bg-gray-800 border-2 border-green-400',
    textStyle: 'text-green-600',
  },
};

export const GymMapScreen: React.FC<Props> = ({ navigation }) => {
  const [gyms, setGyms] = useState<GymLeaderDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const load = async () => {
    try {
      const data = await fetchGyms();
      setGyms(data.sort((a, b) => a.gymOrder - b.gymOrder));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => { load(); }, []);

  const GymCard = ({ gym }: { gym: GymLeaderDto }) => {
    const cfg = statusConfig[gym.status] ?? statusConfig[GymStatus.Locked];
    const isLocked = gym.status === GymStatus.Locked;

    return (
      <TouchableOpacity
        className={`flex-row items-center rounded-2xl p-4 mb-3 mx-4 ${cfg.style}`}
        activeOpacity={isLocked ? 1 : 0.85}
        onPress={() => {
          if (!isLocked) {
            navigation.navigate('ArenaBattle', { gymId: gym.id });
          }
        }}
      >
        {/* Gym avatar */}
        <View className="w-14 h-14 rounded-2xl bg-gray-100 dark:bg-gray-700 items-center justify-center mr-4 overflow-hidden">
          {gym.avatarUrl ? (
            <Image source={{ uri: gym.avatarUrl }} className="w-full h-full" resizeMode="cover" />
          ) : (
            <Text className="text-2xl">{cfg.icon}</Text>
          )}
        </View>

        {/* Info */}
        <View className="flex-1">
          <View className="flex-row items-center justify-between mb-1">
            <Text className="text-gray-900 dark:text-white font-bold text-base">
              #{gym.gymOrder} {gym.name}
            </Text>
            <View className={`px-2 py-0.5 rounded-full ${
              gym.status === GymStatus.Defeated ? 'bg-green-100' :
              gym.status === GymStatus.Unlocked ? 'bg-blue-100' : 'bg-gray-100'
            }`}>
              <Text className={`text-xs font-bold ${cfg.textStyle}`}>
                {cfg.label}
              </Text>
            </View>
          </View>
          <Text className="text-gray-500 dark:text-gray-400 text-sm">{gym.title}</Text>
          <Text className="text-gray-400 text-xs mt-1">Chủ đề: {gym.theme}</Text>

          {/* Unlock progress */}
          {isLocked && (
            <View className="mt-2">
              <View className="flex-row justify-between">
                <Text className="text-gray-400 text-xs">
                  XP: {gym.currentXp}/{gym.xpThreshold}
                </Text>
                <Text className="text-gray-400 text-xs">
                  Từ: {gym.currentVocabCount}/{gym.vocabThreshold}
                </Text>
              </View>
              <ProgressBar
                value={(gym.currentXp / gym.xpThreshold) * 100}
                color="#9ca3af"
                height={5}
                animated={false}
              />
            </View>
          )}

          {/* Cooldown */}
          {gym.isOnCooldown && gym.cooldownEndsAt && (
            <Text className="text-orange-500 text-xs mt-1">
              ⏳ Cooldown: {new Date(gym.cooldownEndsAt).toLocaleTimeString('vi-VN')}
            </Text>
          )}

          {/* Best score */}
          {gym.status === GymStatus.Defeated && (
            <Text className="text-green-600 text-xs mt-1">
              🏅 Điểm cao nhất: {gym.bestScore}/{gym.questionCount}
            </Text>
          )}
        </View>
      </TouchableOpacity>
    );
  };

  return (
    <SafeAreaView className="flex-1 bg-gray-50 dark:bg-gray-950">
      {/* Header */}
      <View className="flex-row items-center justify-between px-4 py-3">
        <Text className="text-gray-900 dark:text-white text-xl font-black">
          ⚔️ Thử thách
        </Text>
        <TouchableOpacity
          className="bg-violet-600 px-4 py-2 rounded-xl"
          onPress={() => navigation.navigate('PvpLobby')}
        >
          <Text className="text-white font-bold text-sm">PvP 🆚</Text>
        </TouchableOpacity>
      </View>

      {loading ? (
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator size="large" color="#3b82f6" />
        </View>
      ) : (
        <ScrollView
          showsVerticalScrollIndicator={false}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={() => { setRefreshing(true); load(); }}
            />
          }
          contentContainerStyle={{ paddingVertical: 8, paddingBottom: 24 }}
        >
          {gyms.map((gym) => <GymCard key={gym.id} gym={gym} />)}
        </ScrollView>
      )}
    </SafeAreaView>
  );
};
