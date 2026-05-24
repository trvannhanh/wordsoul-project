import React, { useCallback, useEffect, useState } from 'react';
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  TextInput,
  RefreshControl,
  Image,
  ActivityIndicator,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { LearnStackParamList } from '../../navigation/MainTabs';
import { fetchGroupedVocabularySets, fetchUserVocabularySets } from '../../services/vocabularySet';
import type { VocabularySetDto } from '../../types/VocabularySetDto';
import { VocabularySetThemeLabel } from '../../types/VocabularySetDto';
import { Ionicons } from '@expo/vector-icons';
import { useDebounce } from 'use-debounce';

type Props = NativeStackScreenProps<LearnStackParamList, 'VocabSetList'>;

const TABS = ['Khám phá', 'Đang học', 'Của tôi'] as const;
type Tab = (typeof TABS)[number];

export const VocabSetListScreen: React.FC<Props> = ({ navigation }) => {
  const [activeTab, setActiveTab] = useState<Tab>('Khám phá');
  const [search, setSearch] = useState('');
  const [debouncedSearch] = useDebounce(search, 400);
  const [grouped, setGrouped] = useState<Record<string, VocabularySetDto[]>>({});
  const [mySetsList, setMySetsList] = useState<VocabularySetDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadData = useCallback(async () => {
    try {
      if (activeTab === 'Khám phá') {
        const data = await fetchGroupedVocabularySets(debouncedSearch || undefined);
        setGrouped(data);
      } else {
        const isOwned = activeTab === 'Đang học' ? true : undefined;
        const data = await fetchUserVocabularySets({
          title: debouncedSearch || undefined,
          isOwned,
        });
        setMySetsList(data);
      }
    } catch {
      // silent
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [activeTab, debouncedSearch]);

  useEffect(() => {
    setLoading(true);
    loadData();
  }, [loadData]);

  const VocabSetCard = ({ item }: { item: VocabularySetDto }) => (
    <TouchableOpacity
      className="flex-1 bg-white dark:bg-gray-800 rounded-2xl overflow-hidden mr-3 mb-3"
      activeOpacity={0.85}
      onPress={() =>
        navigation.navigate('VocabSetDetail', { setId: item.id, title: item.title })
      }
    >
      {item.imageUrl ? (
        <Image
          source={{ uri: item.imageUrl }}
          className="w-full h-28"
          resizeMode="cover"
        />
      ) : (
        <View className="w-full h-28 bg-gradient-to-br from-blue-400 to-violet-500 items-center justify-center">
          <Text className="text-4xl">📚</Text>
        </View>
      )}
      <View className="p-3">
        <Text
          className="text-gray-900 dark:text-white font-bold text-sm"
          numberOfLines={2}
        >
          {item.title}
        </Text>
        <View className="flex-row items-center mt-1 gap-x-2">
          <View className="bg-blue-100 dark:bg-blue-900/40 px-2 py-0.5 rounded-full">
            <Text className="text-blue-600 dark:text-blue-300 text-xs">
              {item.difficultyLevel}
            </Text>
          </View>
          {item.isOwned && (
            <Ionicons name="checkmark-circle" size={14} color="#22c55e" />
          )}
        </View>
      </View>
    </TouchableOpacity>
  );

  const renderGrouped = () =>
    Object.entries(grouped).map(([theme, sets]) => (
      <View key={theme} className="mb-6">
        <Text className="text-gray-900 dark:text-white font-bold text-base mb-3 px-4">
          {VocabularySetThemeLabel[theme] ?? theme}
        </Text>
        <FlatList
          data={sets}
          keyExtractor={(s) => String(s.id)}
          horizontal
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={{ paddingHorizontal: 16 }}
          renderItem={({ item }) => (
            <TouchableOpacity
              className="w-40 bg-white dark:bg-gray-800 rounded-2xl overflow-hidden mr-3"
              activeOpacity={0.85}
              onPress={() =>
                navigation.navigate('VocabSetDetail', {
                  setId: item.id,
                  title: item.title,
                })
              }
            >
              {item.imageUrl ? (
                <Image
                  source={{ uri: item.imageUrl }}
                  className="w-full h-24"
                  resizeMode="cover"
                />
              ) : (
                <View className="w-full h-24 bg-blue-100 items-center justify-center">
                  <Text className="text-3xl">📖</Text>
                </View>
              )}
              <View className="p-2">
                <Text
                  className="text-gray-900 dark:text-white text-xs font-semibold"
                  numberOfLines={2}
                >
                  {item.title}
                </Text>
              </View>
            </TouchableOpacity>
          )}
        />
      </View>
    ));

  return (
    <SafeAreaView className="flex-1 bg-gray-50 dark:bg-gray-950">
      {/* Search */}
      <View className="px-4 pt-3 pb-2">
        <Text className="text-gray-900 dark:text-white text-xl font-black mb-3">
          Bộ từ vựng
        </Text>
        <View className="flex-row items-center bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl px-3 py-2">
          <Ionicons name="search" size={18} color="#9ca3af" />
          <TextInput
            className="flex-1 ml-2 text-sm text-gray-900 dark:text-white"
            placeholder="Tìm kiếm bộ từ vựng..."
            placeholderTextColor="#9ca3af"
            value={search}
            onChangeText={setSearch}
          />
          {search.length > 0 && (
            <TouchableOpacity onPress={() => setSearch('')}>
              <Ionicons name="close-circle" size={18} color="#9ca3af" />
            </TouchableOpacity>
          )}
        </View>
      </View>

      {/* Tabs */}
      <View className="flex-row px-4 mb-3 gap-x-2">
        {TABS.map((tab) => (
          <TouchableOpacity
            key={tab}
            className={`px-4 py-2 rounded-full border ${
              activeTab === tab
                ? 'bg-blue-600 border-blue-600'
                : 'bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700'
            }`}
            onPress={() => setActiveTab(tab)}
          >
            <Text
              className={`text-sm font-semibold ${
                activeTab === tab ? 'text-white' : 'text-gray-600 dark:text-gray-400'
              }`}
            >
              {tab}
            </Text>
          </TouchableOpacity>
        ))}
      </View>

      {/* Content */}
      {loading ? (
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator size="large" color="#3b82f6" />
        </View>
      ) : activeTab === 'Khám phá' ? (
        <FlatList
          data={[]}
          keyExtractor={() => ''}
          renderItem={null}
          ListHeaderComponent={<>{renderGrouped()}</>}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={() => {
                setRefreshing(true);
                loadData();
              }}
            />
          }
          contentContainerStyle={{ paddingBottom: 24 }}
          showsVerticalScrollIndicator={false}
        />
      ) : (
        <FlatList
          data={mySetsList}
          keyExtractor={(item) => String(item.id)}
          numColumns={2}
          contentContainerStyle={{ padding: 16, paddingBottom: 24 }}
          columnWrapperStyle={{ gap: 12 }}
          renderItem={({ item }) => <VocabSetCard item={item} />}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={() => {
                setRefreshing(true);
                loadData();
              }}
            />
          }
          ListEmptyComponent={
            <View className="items-center py-16">
              <Text className="text-4xl mb-3">📭</Text>
              <Text className="text-gray-500 dark:text-gray-400">
                Chưa có bộ từ nào
              </Text>
            </View>
          }
          showsVerticalScrollIndicator={false}
        />
      )}
    </SafeAreaView>
  );
};
