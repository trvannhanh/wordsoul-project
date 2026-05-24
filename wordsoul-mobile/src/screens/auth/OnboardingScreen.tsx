import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  Alert,
  Image,
  ActivityIndicator,
} from 'react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { AuthStackParamList } from '../../navigation/AuthStack';
import { useAuth } from '../../contexts/AuthContext';
import { fetchAllPets } from '../../services/pet';
import type { PetDto } from '../../types/PetDto';
import { Button } from '../../components/ui/Button';
import { Ionicons } from '@expo/vector-icons';
import { StatusBar } from 'expo-status-bar';

type Props = NativeStackScreenProps<AuthStackParamList, 'Onboarding'>;

export const OnboardingScreen: React.FC<Props> = ({ navigation, route }) => {
  const { username, email, password } = route.params;

  const { register } = useAuth();
  const [starterPets, setStarterPets] = useState<PetDto[]>([]);
  const [selected, setSelected] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);
  const [registering, setRegistering] = useState(false);

  useEffect(() => {
    fetchAllPets()
      .then((pets) => setStarterPets(pets.slice(0, 3)))
      .catch(() => setStarterPets([]))
      .finally(() => setLoading(false));
  }, []);

  const handleConfirm = async () => {
    setRegistering(true);
    try {
      await register(
        username,
        email,
        password,
        selected ?? undefined,
      );
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Đăng ký thất bại';
      Alert.alert('Đăng ký thất bại', msg);
      setRegistering(false);
    }
  };

  const rarityColor: Record<string, string> = {
    Common: '#6b7280',
    Uncommon: '#22c55e',
    Rare: '#3b82f6',
    Epic: '#8b5cf6',
    Legendary: '#f59e0b',
  };

  return (
    <View className="flex-1 bg-white dark:bg-gray-950">
      <StatusBar style="dark" />
      <ScrollView
        contentContainerStyle={{ flexGrow: 1 }}
        showsVerticalScrollIndicator={false}
      >
        {/* Back */}
        <TouchableOpacity
          className="mt-14 mx-6 self-start"
          onPress={() => navigation.goBack()}
        >
          <Ionicons name="arrow-back" size={24} color="#374151" />
        </TouchableOpacity>

        <View className="px-6 pt-6">
          <Text className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
            Chọn người đồng hành ✨
          </Text>
          <Text className="text-gray-500 dark:text-gray-400 mb-8 text-base">
            Pet đồng hành sẽ hỗ trợ bạn trong quá trình học. Bạn có thể thu thập thêm sau!
          </Text>

          {loading ? (
            <View className="items-center py-16">
              <ActivityIndicator size="large" color="#3b82f6" />
            </View>
          ) : (
            <View className="flex-row flex-wrap justify-between gap-y-4">
              {starterPets.map((pet) => {
                const isSelected = selected === pet.id;
                return (
                  <TouchableOpacity
                    key={pet.id}
                    className={`w-[31%] rounded-2xl p-3 items-center border-2 ${
                      isSelected
                        ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20'
                        : 'border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800'
                    }`}
                    onPress={() => setSelected(isSelected ? null : pet.id)}
                    activeOpacity={0.8}
                  >
                    {isSelected && (
                      <View className="absolute top-2 right-2 z-10 bg-blue-500 rounded-full w-5 h-5 items-center justify-center">
                        <Ionicons name="checkmark" size={12} color="white" />
                      </View>
                    )}
                    <Image
                      source={{ uri: pet.imageUrl }}
                      className="w-16 h-16"
                      resizeMode="contain"
                    />
                    <Text className="text-gray-900 dark:text-white font-bold text-sm mt-2 text-center">
                      {pet.name}
                    </Text>
                    <View
                      className="px-2 py-0.5 rounded-full mt-1"
                      style={{
                        backgroundColor: `${rarityColor[pet.rarity] ?? '#6b7280'}20`,
                      }}
                    >
                      <Text
                        className="text-xs font-semibold"
                        style={{ color: rarityColor[pet.rarity] ?? '#6b7280' }}
                      >
                        {pet.type}
                      </Text>
                    </View>
                  </TouchableOpacity>
                );
              })}
            </View>
          )}

          {/* Skip note */}
          <Text className="text-center text-gray-400 text-sm mt-6">
            Bỏ qua nếu muốn chọn sau
          </Text>
        </View>
      </ScrollView>

      {/* Bottom CTA */}
      <View className="px-6 pb-8 pt-4 border-t border-gray-100 dark:border-gray-800">
        <Button
          title={selected ? 'Bắt đầu hành trình 🚀' : 'Bỏ qua và bắt đầu →'}
          onPress={handleConfirm}
          loading={registering}
          fullWidth
          size="lg"
          variant={selected ? 'primary' : 'outline'}
        />
      </View>
    </View>
  );
};
