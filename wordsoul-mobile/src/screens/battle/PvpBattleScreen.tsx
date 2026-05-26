import React from 'react';
import { View, Text } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { BattleStackParamList } from '../../navigation/MainTabs';
import { Button } from '../../components/ui/Button';

type Props = NativeStackScreenProps<BattleStackParamList, 'PvpBattle'>;

export const PvpBattleScreen: React.FC<Props> = ({ navigation }) => (
  <SafeAreaView className="flex-1 bg-gray-900 items-center justify-center px-6">
    <Text className="text-white text-4xl font-black mb-4">⚡ PvP Battle</Text>
    <Text className="text-gray-400 text-sm text-center mb-8">
      Real-time battle screen sẽ kết nối SignalR Hub và xử lý BattleStartedDto,
      RoundQuestionDto, RoundResultDto, BattleEndedDto events.
    </Text>
    <Button
      title="Thoát"
      variant="danger"
      onPress={() => navigation.popToTop()}
    />
  </SafeAreaView>
);
