import React from 'react';
import { View, Text, ScrollView, TouchableOpacity } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useAuth } from '../../contexts/AuthContext';
import { Ionicons } from '@expo/vector-icons';

type SettingRow = { icon: string; label: string; onPress: () => void; value?: string };

export const SettingsScreen: React.FC = () => {
  const { logout } = useAuth();

  const Section = ({
    title,
    rows,
  }: {
    title: string;
    rows: SettingRow[];
  }) => (
    <View className="mb-5">
      <Text className="text-gray-500 dark:text-gray-400 text-xs font-bold uppercase px-4 mb-2">
        {title}
      </Text>
      <View className="bg-white dark:bg-gray-800 rounded-2xl mx-4 overflow-hidden">
        {rows.map((row, i) => (
          <TouchableOpacity
            key={i}
            className={`flex-row items-center px-4 py-4 ${
              i < rows.length - 1 ? 'border-b border-gray-100 dark:border-gray-700' : ''
            }`}
            onPress={row.onPress}
            activeOpacity={0.7}
          >
            <Text className="text-xl mr-3">{row.icon}</Text>
            <Text className="flex-1 text-gray-800 dark:text-gray-200 text-base">{row.label}</Text>
            {row.value && (
              <Text className="text-gray-400 text-sm mr-2">{row.value}</Text>
            )}
            <Ionicons name="chevron-forward" size={16} color="#9ca3af" />
          </TouchableOpacity>
        ))}
      </View>
    </View>
  );

  return (
    <SafeAreaView className="flex-1 bg-gray-50 dark:bg-gray-950">
      <Text className="text-gray-900 dark:text-white text-xl font-black px-4 py-3">
        ⚙️ Cài đặt
      </Text>
      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={{ paddingBottom: 32 }}>
        <Section
          title="Tài khoản"
          rows={[
            { icon: '👤', label: 'Thông tin hồ sơ', onPress: () => {} },
            { icon: '🔑', label: 'Đổi mật khẩu', onPress: () => {} },
          ]}
        />
        <Section
          title="Thông báo"
          rows={[
            { icon: '🔔', label: 'Thông báo đẩy', onPress: () => {} },
            { icon: '📅', label: 'Nhắc nhở học hằng ngày', onPress: () => {} },
          ]}
        />
        <Section
          title="Giao diện"
          rows={[
            { icon: '🌙', label: 'Chế độ tối', onPress: () => {} },
            { icon: '🌐', label: 'Ngôn ngữ', value: 'Tiếng Việt', onPress: () => {} },
          ]}
        />
        <Section
          title="Khác"
          rows={[
            { icon: '📖', label: 'Điều khoản dịch vụ', onPress: () => {} },
            { icon: '🔒', label: 'Chính sách quyền riêng tư', onPress: () => {} },
            { icon: 'ℹ️', label: 'Phiên bản ứng dụng', value: '1.0.0', onPress: () => {} },
          ]}
        />
        <Section
          title="Tài khoản"
          rows={[
            { icon: '🚪', label: 'Đăng xuất', onPress: logout },
          ]}
        />
      </ScrollView>
    </SafeAreaView>
  );
};
