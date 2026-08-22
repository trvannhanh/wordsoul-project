import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  Image,
  ActivityIndicator,
  Alert,
  FlatList,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { LearnStackParamList } from '../../navigation/MainTabs';
import {
  fetchVocabularySetDetail,
  fetchMyProgress,
  registerVocabularySet,
  unregisterVocabularySet,
} from '../../services/vocabularySet';
import { createLearningSession } from '../../services/learningSession';
import type { VocabularySetDetailDto } from '../../types/VocabularySetDto';
import type { VocabularySetProgressDto } from '../../types/VocabularySetDto';
import type { VocabularyDto } from '../../types/VocabularyDto';
import { ProgressBar } from '../../components/ui/ProgressBar';
import { Button } from '../../components/ui/Button';
import { Ionicons } from '@expo/vector-icons';

type Props = NativeStackScreenProps<LearnStackParamList, 'VocabSetDetail'>;

export const VocabSetDetailScreen: React.FC<Props> = ({ navigation, route }) => {
  const { setId, title } = route.params;
  const [detail, setDetail] = useState<VocabularySetDetailDto | null>(null);
  const [myProgress, setMyProgress] = useState<VocabularySetProgressDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);

  useEffect(() => {
    const load = async () => {
      try {
        const [d, p] = await Promise.allSettled([
          fetchVocabularySetDetail(setId),
          fetchMyProgress(setId),
        ]);
        if (d.status === 'fulfilled') setDetail(d.value);
        if (p.status === 'fulfilled') setMyProgress(p.value);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [setId]);

  const handleStartLearning = async () => {
    if (!detail) return;
    setActionLoading(true);
    try {
      // Register if not owned
      if (!detail.isOwned) {
        await registerVocabularySet(setId);
      }
      const session = await createLearningSession(setId);
      navigation.navigate('LearningSession', {
        sessionId: session.id,
        vocabSetId: setId,
        mode: 'learning',
      });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Không thể bắt đầu phiên học';
      Alert.alert('Lỗi', msg);
    } finally {
      setActionLoading(false);
    }
  };

  const VocabItem = ({ item }: { item: VocabularyDto }) => (
    <View className="flex-row items-start py-3 border-b border-gray-100 dark:border-gray-800">
      <View className="flex-1">
        <View className="flex-row items-center gap-x-2">
          <Text className="text-gray-900 dark:text-white font-bold">{item.word}</Text>
          {item.partOfSpeech && (
            <Text className="text-gray-400 text-xs italic">{item.partOfSpeech}</Text>
          )}
          {item.cefrLevel && (
            <View className="bg-blue-100 dark:bg-blue-900/30 px-1.5 py-0.5 rounded">
              <Text className="text-blue-600 text-xs font-bold">{item.cefrLevel}</Text>
            </View>
          )}
        </View>
        <Text className="text-gray-600 dark:text-gray-400 text-sm mt-0.5">
          {item.meaning}
        </Text>
        {item.pronunciation && (
          <Text className="text-gray-400 text-xs mt-0.5">/{item.pronunciation}/</Text>
        )}
      </View>
      {item.imageUrl && (
        <Image
          source={{ uri: item.imageUrl }}
          className="w-12 h-12 rounded-lg ml-3"
          resizeMode="cover"
        />
      )}
    </View>
  );

  if (loading) {
    return (
      <SafeAreaView className="flex-1 bg-white dark:bg-gray-950 items-center justify-center">
        <ActivityIndicator size="large" color="#3b82f6" />
      </SafeAreaView>
    );
  }

  const progressPercent = myProgress
    ? (myProgress.learnedVocabularies / myProgress.totalVocabularies) * 100
    : 0;

  return (
    <SafeAreaView className="flex-1 bg-white dark:bg-gray-950">
      <FlatList
        data={detail?.vocabularies ?? []}
        keyExtractor={(item) => String(item.id)}
        renderItem={({ item }) => <VocabItem item={item} />}
        contentContainerStyle={{ paddingHorizontal: 16, paddingBottom: 120 }}
        showsVerticalScrollIndicator={false}
        ListHeaderComponent={
          <>
            {/* Back */}
            <TouchableOpacity
              className="py-3 self-start"
              onPress={() => navigation.goBack()}
            >
              <Ionicons name="arrow-back" size={24} color="#374151" />
            </TouchableOpacity>

            {/* Hero image */}
            {detail?.imageUrl && (
              <Image
                source={{ uri: detail.imageUrl }}
                className="w-full h-44 rounded-2xl mb-4"
                resizeMode="cover"
              />
            )}

            {/* Title & meta */}
            <Text className="text-gray-900 dark:text-white text-2xl font-black mb-1">
              {detail?.title ?? title}
            </Text>
            {detail?.description && (
              <Text className="text-gray-500 dark:text-gray-400 text-sm mb-3">
                {detail.description}
              </Text>
            )}

            <View className="flex-row gap-x-2 mb-4">
              <View className="bg-blue-100 dark:bg-blue-900/30 px-3 py-1 rounded-full">
                <Text className="text-blue-600 text-xs font-semibold">
                  {detail?.difficultyLevel}
                </Text>
              </View>
              <View className="bg-violet-100 dark:bg-violet-900/30 px-3 py-1 rounded-full">
                <Text className="text-violet-600 text-xs font-semibold">
                  {detail?.theme}
                </Text>
              </View>
              {detail?.isPublic && (
                <View className="bg-green-100 px-3 py-1 rounded-full">
                  <Text className="text-green-600 text-xs font-semibold">Công khai</Text>
                </View>
              )}
            </View>

            {/* Progress */}
            {myProgress && (
              <View className="bg-gray-50 dark:bg-gray-900 rounded-2xl p-4 mb-4">
                <View className="flex-row justify-between mb-2">
                  <Text className="text-gray-700 dark:text-gray-300 font-semibold">
                    Tiến độ của bạn
                  </Text>
                  <Text className="text-blue-600 font-bold">
                    {myProgress.learnedVocabularies}/{myProgress.totalVocabularies}
                  </Text>
                </View>
                <ProgressBar value={progressPercent} color="#3b82f6" height={10} />
              </View>
            )}

            {/* Stats */}
            <View className="flex-row justify-around bg-gray-50 dark:bg-gray-900 rounded-2xl py-4 mb-6">
              <View className="items-center">
                <Text className="text-2xl font-black text-gray-900 dark:text-white">
                  {detail?.totalVocabularies ?? 0}
                </Text>
                <Text className="text-gray-400 text-xs">Từ vựng</Text>
              </View>
              <View className="w-px bg-gray-200 dark:bg-gray-700" />
              <View className="items-center">
                <Text className="text-2xl font-black text-gray-900 dark:text-white">
                  {detail?.createdByUsername ?? '—'}
                </Text>
                <Text className="text-gray-400 text-xs">Tác giả</Text>
              </View>
            </View>

            <Text className="text-gray-900 dark:text-white font-bold text-base mb-2">
              Danh sách từ vựng
            </Text>
          </>
        }
      />

      {/* Fixed bottom CTA */}
      <View className="absolute bottom-0 left-0 right-0 bg-white dark:bg-gray-950 px-5 pb-8 pt-4 border-t border-gray-100 dark:border-gray-800">
        <Button
          title={detail?.isOwned ? '📖 Tiếp tục học' : '🚀 Bắt đầu học'}
          onPress={handleStartLearning}
          loading={actionLoading}
          fullWidth
          size="lg"
        />
      </View>
    </SafeAreaView>
  );
};
