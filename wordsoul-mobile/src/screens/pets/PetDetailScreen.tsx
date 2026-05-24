import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  Image,
  ActivityIndicator,
  Alert,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { PetsStackParamList } from '../../navigation/MainTabs';
import { fetchPetDetail, setActivePet, upgradePet } from '../../services/pet';
import type { PetDetailDto } from '../../types/PetDto';
import { rarityColors, typeColors } from '../../types/PetDto';
import { ProgressBar } from '../../components/ui/ProgressBar';
import { Button } from '../../components/ui/Button';
import { Ionicons } from '@expo/vector-icons';
import { useAuth } from '../../contexts/AuthContext';
import { getCurrentUser } from '../../services/user';

type Props = NativeStackScreenProps<PetsStackParamList, 'PetDetail'>;

const XP_PER_LEVEL = 100;

export const PetDetailScreen: React.FC<Props> = ({ navigation, route }) => {
  const { petId } = route.params;
  const { user, setUser } = useAuth();
  const [pet, setPet] = useState<PetDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [activating, setActivating] = useState(false);
  const [upgrading, setUpgrading] = useState(false);

  useEffect(() => {
    fetchPetDetail(petId)
      .then(setPet)
      .catch(() => Alert.alert('Lỗi', 'Không thể tải thông tin pet'))
      .finally(() => setLoading(false));
  }, [petId]);

  const handleSetActive = async () => {
    setActivating(true);
    try {
      await setActivePet(petId);
      const me = await getCurrentUser();
      setUser(me);
      Alert.alert('Thành công', `${pet?.name} đã được đặt làm active!`);
    } catch {
      Alert.alert('Lỗi', 'Không thể thay đổi pet active');
    } finally {
      setActivating(false);
    }
  };

  const handleUpgrade = async () => {
    if (!pet) return;
    setUpgrading(true);
    try {
      const res = await upgradePet(petId);
      const updated = await fetchPetDetail(petId);
      setPet(updated);
      const msg = res.isEvolved
        ? `🎊 ${pet.name} đã tiến hóa!`
        : res.isLevelUp
        ? `⬆️ Lên cấp ${res.level}!`
        : `+${res.experience} XP`;
      Alert.alert('Upgrade!', msg);
    } catch {
      Alert.alert('Lỗi', 'Không thể upgrade pet');
    } finally {
      setUpgrading(false);
    }
  };

  if (loading || !pet) {
    return (
      <SafeAreaView className="flex-1 bg-white dark:bg-gray-950 items-center justify-center">
        <ActivityIndicator size="large" color="#3b82f6" />
      </SafeAreaView>
    );
  }

  const isActive = user?.petActiveId === petId;
  const rColor = rarityColors[pet.rarity] ?? '#6b7280';
  const tColor = typeColors[pet.type] ?? '#6b7280';
  const xpPercent = pet.experience != null
    ? ((pet.experience % XP_PER_LEVEL) / XP_PER_LEVEL) * 100
    : 0;

  const buffs = [
    pet.acquiredAt && { icon: '📅', label: 'Sở hữu từ', value: new Date(pet.acquiredAt).toLocaleDateString('vi-VN') },
  ].filter(Boolean);

  return (
    <SafeAreaView className="flex-1 bg-white dark:bg-gray-950">
      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={{ paddingBottom: 120 }}>
        {/* Back */}
        <TouchableOpacity
          className="px-4 py-3 self-start"
          onPress={() => navigation.goBack()}
        >
          <Ionicons name="arrow-back" size={24} color="#374151" />
        </TouchableOpacity>

        {/* Hero */}
        <View
          className="mx-4 rounded-3xl py-10 items-center mb-6"
          style={{ backgroundColor: `${tColor}20` }}
        >
          {isActive && (
            <View className="absolute top-4 right-4 bg-blue-500 rounded-full px-3 py-1">
              <Text className="text-white text-xs font-bold">✓ Active</Text>
            </View>
          )}
          <Image
            source={{ uri: pet.imageUrl }}
            className="w-32 h-32"
            resizeMode="contain"
          />
          <Text className="text-2xl font-black text-gray-900 dark:text-white mt-4">
            {pet.name}
          </Text>
          <View className="flex-row gap-x-2 mt-2">
            <View className="px-3 py-1 rounded-full" style={{ backgroundColor: `${rColor}20` }}>
              <Text className="text-sm font-bold" style={{ color: rColor }}>{pet.rarity}</Text>
            </View>
            <View className="px-3 py-1 rounded-full" style={{ backgroundColor: `${tColor}20` }}>
              <Text className="text-sm font-bold" style={{ color: tColor }}>{pet.type}</Text>
            </View>
            {pet.secondaryType && (
              <View className="px-3 py-1 rounded-full bg-gray-100 dark:bg-gray-700">
                <Text className="text-sm font-bold text-gray-600 dark:text-gray-400">
                  {pet.secondaryType}
                </Text>
              </View>
            )}
          </View>
        </View>

        {/* Stats */}
        {pet.level != null && (
          <View className="mx-4 bg-gray-50 dark:bg-gray-900 rounded-2xl p-4 mb-4">
            <View className="flex-row justify-between mb-3">
              <Text className="text-gray-700 dark:text-gray-300 font-bold">Cấp độ</Text>
              <Text className="text-gray-900 dark:text-white font-black text-lg">
                {pet.level}
              </Text>
            </View>
            <ProgressBar
              value={xpPercent}
              color={tColor}
              height={10}
              showLabel
              label={`${pet.experience ?? 0} XP`}
            />
            {pet.nextEvolutionId && pet.requiredLevel && (
              <Text className="text-gray-400 text-xs mt-2">
                Tiến hóa tại cấp {pet.requiredLevel}
              </Text>
            )}
          </View>
        )}

        {/* Description */}
        <View className="mx-4 mb-4">
          <Text className="text-gray-600 dark:text-gray-400 leading-6">
            {pet.description}
          </Text>
        </View>
      </ScrollView>

      {/* Fixed bottom actions */}
      <View className="absolute bottom-0 left-0 right-0 bg-white dark:bg-gray-950 px-4 pb-8 pt-4 border-t border-gray-100 dark:border-gray-800 flex-row gap-x-3">
        {!isActive && (
          <Button
            title="Đặt làm active"
            variant="outline"
            onPress={handleSetActive}
            loading={activating}
            className="flex-1"
          />
        )}
        <Button
          title="⬆️ Upgrade"
          onPress={handleUpgrade}
          loading={upgrading}
          className="flex-1"
        />
      </View>
    </SafeAreaView>
  );
};
