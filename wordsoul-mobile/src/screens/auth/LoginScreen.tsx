import React, { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  KeyboardAvoidingView,
  Platform,
  Alert,
  ScrollView,
} from 'react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { AuthStackParamList } from '../../navigation/AuthStack';
import { useAuth } from '../../contexts/AuthContext';
import { Button } from '../../components/ui/Button';
import { Ionicons } from '@expo/vector-icons';
import { StatusBar } from 'expo-status-bar';

type Props = NativeStackScreenProps<AuthStackParamList, 'Login'>;

export const LoginScreen: React.FC<Props> = ({ navigation }) => {
  const { login } = useAuth();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<{ username?: string; password?: string }>({});

  const validate = () => {
    const errs: typeof errors = {};
    if (!username.trim()) errs.username = 'Vui lòng nhập tên đăng nhập';
    if (!password) errs.password = 'Vui lòng nhập mật khẩu';
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleLogin = async () => {
    if (!validate()) return;
    setLoading(true);
    try {
      await login(username.trim(), password);
    } catch (err: unknown) {
      const msg =
        err instanceof Error ? err.message : 'Tên đăng nhập hoặc mật khẩu không đúng';
      Alert.alert('Đăng nhập thất bại', msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <KeyboardAvoidingView
      className="flex-1 bg-white dark:bg-gray-950"
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <StatusBar style="dark" />
      <ScrollView
        contentContainerStyle={{ flexGrow: 1 }}
        keyboardShouldPersistTaps="handled"
        showsVerticalScrollIndicator={false}
      >
        {/* Header */}
        <View className="items-center pt-20 pb-10 px-6">
          <View className="w-20 h-20 bg-blue-600 rounded-3xl items-center justify-center mb-4 shadow-lg">
            <Text className="text-white text-4xl">🎯</Text>
          </View>
          <Text className="text-3xl font-bold text-gray-900 dark:text-white">
            WordSoul
          </Text>
          <Text className="text-gray-500 dark:text-gray-400 mt-2 text-base text-center">
            Học từ vựng tiếng Anh theo phong cách game
          </Text>
        </View>

        {/* Form */}
        <View className="px-6 flex-1">
          {/* Username */}
          <View className="mb-4">
            <Text className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-2">
              Tên đăng nhập
            </Text>
            <View
              className={`flex-row items-center border-2 rounded-xl px-4 py-3 bg-gray-50 dark:bg-gray-800 ${
                errors.username ? 'border-red-400' : 'border-gray-200 dark:border-gray-700'
              }`}
            >
              <Ionicons name="person-outline" size={20} color="#9ca3af" />
              <TextInput
                className="flex-1 ml-3 text-base text-gray-900 dark:text-white"
                placeholder="Nhập tên đăng nhập"
                placeholderTextColor="#9ca3af"
                value={username}
                onChangeText={(t) => {
                  setUsername(t);
                  if (errors.username) setErrors((p) => ({ ...p, username: undefined }));
                }}
                autoCapitalize="none"
                autoCorrect={false}
                returnKeyType="next"
              />
            </View>
            {errors.username && (
              <Text className="text-red-500 text-xs mt-1">{errors.username}</Text>
            )}
          </View>

          {/* Password */}
          <View className="mb-6">
            <Text className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-2">
              Mật khẩu
            </Text>
            <View
              className={`flex-row items-center border-2 rounded-xl px-4 py-3 bg-gray-50 dark:bg-gray-800 ${
                errors.password ? 'border-red-400' : 'border-gray-200 dark:border-gray-700'
              }`}
            >
              <Ionicons name="lock-closed-outline" size={20} color="#9ca3af" />
              <TextInput
                className="flex-1 ml-3 text-base text-gray-900 dark:text-white"
                placeholder="Nhập mật khẩu"
                placeholderTextColor="#9ca3af"
                value={password}
                onChangeText={(t) => {
                  setPassword(t);
                  if (errors.password) setErrors((p) => ({ ...p, password: undefined }));
                }}
                secureTextEntry={!showPassword}
                returnKeyType="done"
                onSubmitEditing={handleLogin}
              />
              <TouchableOpacity onPress={() => setShowPassword((v) => !v)}>
                <Ionicons
                  name={showPassword ? 'eye-off-outline' : 'eye-outline'}
                  size={20}
                  color="#9ca3af"
                />
              </TouchableOpacity>
            </View>
            {errors.password && (
              <Text className="text-red-500 text-xs mt-1">{errors.password}</Text>
            )}
          </View>

          {/* Login Button */}
          <Button
            title="Đăng nhập"
            onPress={handleLogin}
            loading={loading}
            fullWidth
            size="lg"
          />

          {/* Divider */}
          <View className="flex-row items-center my-6">
            <View className="flex-1 h-px bg-gray-200 dark:bg-gray-700" />
            <Text className="mx-4 text-gray-400 text-sm">hoặc</Text>
            <View className="flex-1 h-px bg-gray-200 dark:bg-gray-700" />
          </View>

          {/* Register link */}
          <View className="flex-row justify-center items-center pb-8">
            <Text className="text-gray-600 dark:text-gray-400">
              Chưa có tài khoản?{' '}
            </Text>
            <TouchableOpacity onPress={() => navigation.navigate('Register')}>
              <Text className="text-blue-600 font-bold">Đăng ký ngay</Text>
            </TouchableOpacity>
          </View>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
};
