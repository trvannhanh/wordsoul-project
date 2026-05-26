import React from 'react';
import { SafeAreaView, ScrollView, View, type ViewProps } from 'react-native';

interface ScreenContainerProps extends ViewProps {
  children: React.ReactNode;
  scrollable?: boolean;
  padded?: boolean;
}

export const ScreenContainer: React.FC<ScreenContainerProps> = ({
  children,
  scrollable = true,
  padded = true,
  className,
  ...rest
}) => {
  const innerClass = `flex-1 ${padded ? 'px-4' : ''} ${className ?? ''}`;

  return (
    <SafeAreaView className="flex-1 bg-gray-50 dark:bg-gray-950">
      {scrollable ? (
        <ScrollView
          className="flex-1"
          contentContainerStyle={{ flexGrow: 1 }}
          showsVerticalScrollIndicator={false}
          keyboardShouldPersistTaps="handled"
        >
          <View className={innerClass} {...rest}>
            {children}
          </View>
        </ScrollView>
      ) : (
        <View className={innerClass} {...rest}>
          {children}
        </View>
      )}
    </SafeAreaView>
  );
};
