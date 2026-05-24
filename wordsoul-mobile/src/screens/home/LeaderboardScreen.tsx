import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  RefreshControl,
  Image,
  ActivityIndicator,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { HomeStackParamList } from '../../navigation/MainTabs';
import { getLeaderboard } from '../../services/user';
import { useAuth } from '../../contexts/AuthContext';
import type { UserDto } from '../../types/UserDto';
import { Ionicons } from '@expo/vector-icons';

type Props = NativeStackScreenProps<HomeStackParamList, 'Leaderboard'>;

export const LeaderboardScreen: React.FC<Props> = ({ navigation }) => {
  const { user } = useAuth();
  const [leaders, setLeaders] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const load = async () => {
    try {
      const data = await getLeaderboard();
      setLeaders(data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const medalColors = ['#f59e0b', '#9ca3af', '#cd7c2f'];
  const medalIcons = ['🥇', '🥈', '🥉'];

  const renderItem = ({ item, index }: { item: UserDto; index: number }) => {
    const isMe = item.id === user?.id;
    return (
      <View
        className={`flex-row items-center px-4 py-3 mx-4 mb-2 rounded-2xl ${
          isMe
            ? 'bg-blue-50 dark:bg-blue-900/30 border-2 border-blue-400'
            : 'bg-white dark:bg-gray-800'
        }`}
      >
        <Text className="w-8 text-center text-base font-bold text-gray-500 dark:text-gray-400">
          {index < 3 ? medalIcons[index] : `#${index + 1}`}
        </Text>
        {item.avatarUrl ? (
          <Image
            source={{ uri: item.avatarUrl }}
            className="w-10 h-10 rounded-full mx-3"
          />
        ) : (
          <View className="w-10 h-10 rounded-full bg-blue-100 dark:bg-blue-900 items-center justify-center mx-3">
            <Text className="text-blue-600 font-bold">
              {item.username[0]?.toUpperCase()}
            </Text>
          </View>
        )}
        <View className="flex-1">
          <Text className="text-gray-900 dark:text-white font-semibold">
            {item.username} {isMe && '(Bạn)'}
          </Text>
          <Text className="text-gray-500 dark:text-gray-400 text-xs">
            Cấp {item.level}
          </Text>
        </View>
        <View className="items-end">
          <Text className="text-blue-600 dark:text-blue-400 font-bold">
            {item.totalXP.toLocaleString()} XP
          </Text>
          <Text className="text-orange-500 text-xs">🔥 {item.streakDays}</Text>
        </View>
      </View>
    );
  };

  return (
    <SafeAreaView className="flex-1 bg-gray-50 dark:bg-gray-950">
      {/* Header */}
      <View className="flex-row items-center px-4 py-3">
        <TouchableOpacity onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={24} color="#374151" />
        </TouchableOpacity>
        <Text className="text-gray-900 dark:text-white text-lg font-bold ml-3">
          🏆 Bảng xếp hạng
        </Text>
      </View>

      {loading ? (
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator size="large" color="#3b82f6" />
        </View>
      ) : (
        <FlatList
          data={leaders}
          keyExtractor={(item) => String(item.id)}
          renderItem={renderItem}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={async () => {
                setRefreshing(true);
                await load();
                setRefreshing(false);
              }}
            />
          }
          contentContainerStyle={{ paddingBottom: 24, paddingTop: 8 }}
          showsVerticalScrollIndicator={false}
        />
      )}
    </SafeAreaView>
  );
};
