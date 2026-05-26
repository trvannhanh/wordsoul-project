import React from 'react';
import { View, Text } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { BattleStackParamList } from '../../navigation/MainTabs';
import { Button } from '../../components/ui/Button';

type Props = NativeStackScreenProps<BattleStackParamList, 'PvpLobby'>;

export const PvpLobbyScreen: React.FC<Props> = ({ navigation }) => (
  <SafeAreaView className="flex-1 bg-gray-900 items-center justify-center px-6">
    <Text className="text-white text-4xl font-black mb-4">🆚 PvP</Text>
    <Text className="text-gray-300 text-center mb-6">
      Thách đấu người chơi khác
    </Text>
    <Text className="text-gray-400 text-sm text-center mb-8">
      Tính năng PvP real-time qua SignalR Hub /pvpHub sẽ được triển khai đầy đủ
      trong Phase 5.
    </Text>
    <View className="w-full gap-y-3">
      <Button
        title="🔍 Tìm đối thủ ngẫu nhiên"
        onPress={() => navigation.navigate('PvpBattle', {})}
        fullWidth
      />
      <Button
        title="Quay lại"
        variant="ghost"
        onPress={() => navigation.goBack()}
        fullWidth
      />
    </View>
  </SafeAreaView>
);
