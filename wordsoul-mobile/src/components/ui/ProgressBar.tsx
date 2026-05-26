import React, { useEffect } from 'react';
import { View, Text } from 'react-native';
import Animated, {
  useSharedValue,
  useAnimatedStyle,
  withTiming,
} from 'react-native-reanimated';

interface ProgressBarProps {
  value: number; // 0-100
  color?: string;
  height?: number;
  showLabel?: boolean;
  label?: string;
  animated?: boolean;
}

export const ProgressBar: React.FC<ProgressBarProps> = ({
  value,
  color = '#3b82f6',
  height = 8,
  showLabel = false,
  label,
  animated = true,
}) => {
  const progress = useSharedValue(0);

  useEffect(() => {
    progress.value = withTiming(Math.min(Math.max(value, 0), 100), {
      duration: animated ? 600 : 0,
    });
  }, [value]);

  const animatedStyle = useAnimatedStyle(() => ({
    width: `${progress.value}%`,
  }));

  return (
    <View>
      {(showLabel || label) && (
        <View className="flex-row justify-between mb-1">
          <Text className="text-sm text-gray-600 dark:text-gray-400">
            {label}
          </Text>
          {showLabel && (
            <Text className="text-sm font-semibold text-gray-700 dark:text-gray-300">
              {Math.round(value)}%
            </Text>
          )}
        </View>
      )}
      <View
        className="bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden"
        style={{ height }}
      >
        <Animated.View
          className="h-full rounded-full"
          style={[animatedStyle, { backgroundColor: color }]}
        />
      </View>
    </View>
  );
};
