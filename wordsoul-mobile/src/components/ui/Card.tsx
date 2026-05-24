import React from 'react';
import { View, Text, type ViewProps } from 'react-native';

interface CardProps extends ViewProps {
  title?: string;
  subtitle?: string;
  children: React.ReactNode;
  variant?: 'default' | 'elevated' | 'outlined';
}

export const Card: React.FC<CardProps> = ({
  title,
  subtitle,
  children,
  variant = 'default',
  className,
  ...rest
}) => {
  const variantClass = {
    default: 'bg-white dark:bg-gray-800 rounded-2xl p-4',
    elevated: 'bg-white dark:bg-gray-800 rounded-2xl p-4 shadow-md shadow-black/10',
    outlined: 'bg-white dark:bg-gray-800 rounded-2xl p-4 border border-gray-200 dark:border-gray-700',
  }[variant];

  return (
    <View className={`${variantClass} ${className ?? ''}`} {...rest}>
      {title && (
        <Text className="text-gray-900 dark:text-white text-base font-bold mb-1">
          {title}
        </Text>
      )}
      {subtitle && (
        <Text className="text-gray-500 dark:text-gray-400 text-sm mb-3">
          {subtitle}
        </Text>
      )}
      {children}
    </View>
  );
};
