import React from 'react';
import { View, Text } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { BattleStackParamList } from '../../navigation/MainTabs';
import { Button } from '../../components/ui/Button';

type Props = NativeStackScreenProps<BattleStackParamList, 'ArenaBattle'>;

export const ArenaBattleScreen: React.FC<Props> = ({ navigation, route }) => {
  const { gymId } = route.params;
  return (
    <SafeAreaView className="flex-1 bg-gray-900 items-center justify-center px-6">
      <Text className="text-white text-4xl font-black mb-4">⚔️ Arena</Text>
      <Text className="text-gray-300 text-center mb-8">
        {gymId ? `Gym #${gymId}` : 'Battle Arena'}
      </Text>
      <Text className="text-gray-400 text-center text-sm mb-8">
        Tính năng đang phát triển. Logic chiến đấu với Gym Leader sẽ được triển khai
        với API endpoint /gym/{`{gymId}`}/battle.
      </Text>
      <Button
        title="Quay lại"
        variant="outline"
        onPress={() => navigation.goBack()}
      />
    </SafeAreaView>
  );
};
