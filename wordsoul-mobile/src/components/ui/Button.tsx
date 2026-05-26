import React from 'react';
import {
  TouchableOpacity,
  Text,
  ActivityIndicator,
  type TouchableOpacityProps,
} from 'react-native';

interface ButtonProps extends TouchableOpacityProps {
  title: string;
  variant?: 'primary' | 'secondary' | 'outline' | 'danger' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  loading?: boolean;
  fullWidth?: boolean;
}

const variantClasses: Record<string, { container: string; text: string }> = {
  primary: {
    container: 'bg-blue-600 active:bg-blue-700',
    text: 'text-white font-bold',
  },
  secondary: {
    container: 'bg-violet-600 active:bg-violet-700',
    text: 'text-white font-bold',
  },
  outline: {
    container: 'border-2 border-blue-600 bg-transparent active:bg-blue-50',
    text: 'text-blue-600 font-bold',
  },
  danger: {
    container: 'bg-red-500 active:bg-red-600',
    text: 'text-white font-bold',
  },
  ghost: {
    container: 'bg-transparent active:bg-gray-100',
    text: 'text-gray-700 font-semibold',
  },
};

const sizeClasses: Record<string, { container: string; text: string }> = {
  sm: { container: 'px-4 py-2 rounded-lg', text: 'text-sm' },
  md: { container: 'px-6 py-3 rounded-xl', text: 'text-base' },
  lg: { container: 'px-8 py-4 rounded-2xl', text: 'text-lg' },
};

export const Button: React.FC<ButtonProps> = ({
  title,
  variant = 'primary',
  size = 'md',
  loading = false,
  fullWidth = false,
  disabled,
  className,
  ...rest
}) => {
  const v = variantClasses[variant];
  const s = sizeClasses[size];

  return (
    <TouchableOpacity
      className={`items-center justify-center ${v.container} ${s.container} ${fullWidth ? 'w-full' : ''} ${disabled || loading ? 'opacity-60' : ''} ${className ?? ''}`}
      disabled={disabled || loading}
      activeOpacity={0.8}
      {...rest}
    >
      {loading ? (
        <ActivityIndicator
          size="small"
          color={variant === 'outline' || variant === 'ghost' ? '#3b82f6' : '#ffffff'}
        />
      ) : (
        <Text className={`${v.text} ${s.text}`}>{title}</Text>
      )}
    </TouchableOpacity>
  );
};
