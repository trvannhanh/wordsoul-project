import React, { useCallback, useEffect, useState } from 'react';
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  Image,
  RefreshControl,
  ActivityIndicator,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { PetsStackParamList } from '../../navigation/MainTabs';
import { fetchMyPets } from '../../services/pet';
import { useAuth } from '../../contexts/AuthContext';
import type { PetDto } from '../../types/PetDto';
import { rarityColors, typeColors } from '../../types/PetDto';
import { Ionicons } from '@expo/vector-icons';

type Props = NativeStackScreenProps<PetsStackParamList, 'PetsList'>;

export const PetsScreen: React.FC<Props> = ({ navigation }) => {
  const { user } = useAuth();
  const [pets, setPets] = useState<PetDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const load = useCallback(async () => {
    try {
      const data = await fetchMyPets();
      setPets(data);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const PetCard = ({ item }: { item: PetDto }) => {
    const isActive = item.id === user?.petActiveId;
    const rColor = rarityColors[item.rarity] ?? '#6b7280';
    const tColor = typeColors[item.type] ?? '#6b7280';

    return (
      <TouchableOpacity
        className={`flex-1 bg-white dark:bg-gray-800 rounded-2xl overflow-hidden mr-3 mb-3 border-2 ${
          isActive ? 'border-blue-400' : 'border-transparent'
        }`}
        activeOpacity={0.85}
        onPress={() => navigation.navigate('PetDetail', { petId: item.id })}
      >
        {/* Type color header */}
        <View className="h-24 items-center justify-center" style={{ backgroundColor: `${tColor}20` }}>
          <Image
            source={{ uri: item.imageUrl }}
            className="w-16 h-16"
            resizeMode="contain"
          />
        </View>

        <View className="p-3">
          <View className="flex-row items-center justify-between mb-1">
            <Text
              className="text-gray-900 dark:text-white font-bold text-sm flex-1"
              numberOfLines={1}
            >
              {item.name}
            </Text>
            {isActive && (
              <View className="bg-blue-100 dark:bg-blue-900/40 px-1.5 py-0.5 rounded-full ml-1">
                <Text className="text-blue-600 text-xs font-bold">Active</Text>
              </View>
            )}
          </View>

          <View className="flex-row gap-x-1">
            <View
              className="px-2 py-0.5 rounded-full"
              style={{ backgroundColor: `${rColor}20` }}
            >
              <Text className="text-xs font-semibold" style={{ color: rColor }}>
                {item.rarity}
              </Text>
            </View>
            <View
              className="px-2 py-0.5 rounded-full"
              style={{ backgroundColor: `${tColor}20` }}
            >
              <Text className="text-xs font-semibold" style={{ color: tColor }}>
                {item.type}
              </Text>
            </View>
          </View>
        </View>
      </TouchableOpacity>
    );
  };

  return (
    <SafeAreaView className="flex-1 bg-gray-50 dark:bg-gray-950">
      <View className="flex-row items-center justify-between px-4 py-3">
        <Text className="text-gray-900 dark:text-white text-xl font-black">
          Đồng đội 🐾
        </Text>
        <Text className="text-gray-500 dark:text-gray-400 text-sm">
          {pets.length} pet
        </Text>
      </View>

      {loading ? (
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator size="large" color="#3b82f6" />
        </View>
      ) : (
        <FlatList
          data={pets}
          keyExtractor={(item) => String(item.id)}
          numColumns={2}
          contentContainerStyle={{ padding: 16, paddingBottom: 24 }}
          columnWrapperStyle={{ gap: 12 }}
          renderItem={({ item }) => <PetCard item={item} />}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={() => { setRefreshing(true); load(); }}
            />
          }
          ListEmptyComponent={
            <View className="items-center py-20">
              <Text className="text-5xl mb-4">🥚</Text>
              <Text className="text-gray-500 dark:text-gray-400 text-base font-semibold">
                Chưa có pet nào
              </Text>
              <Text className="text-gray-400 text-sm mt-1 text-center px-8">
                Hãy học từ vựng để bắt pet!
              </Text>
            </View>
          }
          showsVerticalScrollIndicator={false}
        />
      )}
    </SafeAreaView>
  );
};
