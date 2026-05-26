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
import { Ionicons } from '@expo/vector-icons';
import { StatusBar } from 'expo-status-bar';
import { Button } from '../../components/ui/Button';

type Props = NativeStackScreenProps<AuthStackParamList, 'Register'>;

interface FormErrors {
  username?: string;
  email?: string;
  password?: string;
  confirmPassword?: string;
}

export const RegisterScreen: React.FC<Props> = ({ navigation }) => {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [errors, setErrors] = useState<FormErrors>({});

  const validate = (): boolean => {
    const errs: FormErrors = {};
    if (!username.trim() || username.length < 3)
      errs.username = 'Tên đăng nhập ít nhất 3 ký tự';
    if (!email.trim() || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))
      errs.email = 'Email không hợp lệ';
    if (!password || password.length < 6)
      errs.password = 'Mật khẩu ít nhất 6 ký tự';
    if (password !== confirmPassword)
      errs.confirmPassword = 'Mật khẩu không khớp';
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleNext = () => {
    if (!validate()) return;
    navigation.navigate('Onboarding', { username, email, password });
  };

  const InputField = ({
    icon,
    placeholder,
    value,
    onChangeText,
    error,
    secureTextEntry,
    keyboardType,
    showToggle,
    onToggle,
  }: {
    icon: React.ComponentProps<typeof Ionicons>['name'];
    placeholder: string;
    value: string;
    onChangeText: (t: string) => void;
    error?: string;
    secureTextEntry?: boolean;
    keyboardType?: React.ComponentProps<typeof TextInput>['keyboardType'];
    showToggle?: boolean;
    onToggle?: () => void;
  }) => (
    <View className="mb-4">
      <View
        className={`flex-row items-center border-2 rounded-xl px-4 py-3 bg-gray-50 dark:bg-gray-800 ${
          error ? 'border-red-400' : 'border-gray-200 dark:border-gray-700'
        }`}
      >
        <Ionicons name={icon} size={20} color="#9ca3af" />
        <TextInput
          className="flex-1 ml-3 text-base text-gray-900 dark:text-white"
          placeholder={placeholder}
          placeholderTextColor="#9ca3af"
          value={value}
          onChangeText={onChangeText}
          secureTextEntry={secureTextEntry}
          keyboardType={keyboardType}
          autoCapitalize="none"
          autoCorrect={false}
        />
        {showToggle && (
          <TouchableOpacity onPress={onToggle}>
            <Ionicons
              name={secureTextEntry ? 'eye-outline' : 'eye-off-outline'}
              size={20}
              color="#9ca3af"
            />
          </TouchableOpacity>
        )}
      </View>
      {error && (
        <Text className="text-red-500 text-xs mt-1 ml-1">{error}</Text>
      )}
    </View>
  );

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
        {/* Back button */}
        <TouchableOpacity
          className="mt-14 mx-6 self-start"
          onPress={() => navigation.goBack()}
        >
          <Ionicons name="arrow-back" size={24} color="#374151" />
        </TouchableOpacity>

        <View className="px-6 pt-6 flex-1">
          <Text className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
            Tạo tài khoản
          </Text>
          <Text className="text-gray-500 dark:text-gray-400 mb-8">
            Bắt đầu hành trình học từ vựng của bạn
          </Text>

          <InputField
            icon="person-outline"
            placeholder="Tên đăng nhập"
            value={username}
            onChangeText={(t) => {
              setUsername(t);
              setErrors((p) => ({ ...p, username: undefined }));
            }}
            error={errors.username}
          />
          <InputField
            icon="mail-outline"
            placeholder="Email"
            value={email}
            onChangeText={(t) => {
              setEmail(t);
              setErrors((p) => ({ ...p, email: undefined }));
            }}
            error={errors.email}
            keyboardType="email-address"
          />
          <InputField
            icon="lock-closed-outline"
            placeholder="Mật khẩu (tối thiểu 6 ký tự)"
            value={password}
            onChangeText={(t) => {
              setPassword(t);
              setErrors((p) => ({ ...p, password: undefined }));
            }}
            error={errors.password}
            secureTextEntry={!showPassword}
            showToggle
            onToggle={() => setShowPassword((v) => !v)}
          />
          <InputField
            icon="shield-checkmark-outline"
            placeholder="Nhập lại mật khẩu"
            value={confirmPassword}
            onChangeText={(t) => {
              setConfirmPassword(t);
              setErrors((p) => ({ ...p, confirmPassword: undefined }));
            }}
            error={errors.confirmPassword}
            secureTextEntry={!showPassword}
          />

          <Button
            title="Tiếp tục →"
            onPress={handleNext}
            fullWidth
            size="lg"
            className="mt-2"
          />

          <View className="flex-row justify-center items-center py-8">
            <Text className="text-gray-600 dark:text-gray-400">
              Đã có tài khoản?{' '}
            </Text>
            <TouchableOpacity onPress={() => navigation.navigate('Login')}>
              <Text className="text-blue-600 font-bold">Đăng nhập</Text>
            </TouchableOpacity>
          </View>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
};
