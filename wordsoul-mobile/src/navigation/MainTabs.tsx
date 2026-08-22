import React from 'react';
import type { NavigatorScreenParams } from '@react-navigation/native';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { Ionicons } from '@expo/vector-icons';
import { Platform, View } from 'react-native';

// Screens
import { HomeScreen } from '../screens/home/HomeScreen';
import { LeaderboardScreen } from '../screens/home/LeaderboardScreen';
import { VocabSetListScreen } from '../screens/learn/VocabSetListScreen';
import { VocabSetDetailScreen } from '../screens/learn/VocabSetDetailScreen';
import { LearningSessionScreen } from '../screens/learn/LearningSessionScreen';
import { GymMapScreen } from '../screens/battle/GymMapScreen';
import { ArenaBattleScreen } from '../screens/battle/ArenaBattleScreen';
import { PvpLobbyScreen } from '../screens/battle/PvpLobbyScreen';
import { PvpBattleScreen } from '../screens/battle/PvpBattleScreen';
import { PetsScreen } from '../screens/pets/PetsScreen';
import { PetDetailScreen } from '../screens/pets/PetDetailScreen';
import { ProfileScreen } from '../screens/profile/ProfileScreen';
import { AchievementScreen } from '../screens/profile/AchievementScreen';
import { DailyQuestScreen } from '../screens/profile/DailyQuestScreen';
import { SettingsScreen } from '../screens/profile/SettingsScreen';

// ── Param Lists ──────────────────────────────────────────────
export type HomeStackParamList = {
  HomeMain: undefined;
  Leaderboard: undefined;
};

export type LearnStackParamList = {
  VocabSetList: undefined;
  VocabSetDetail: { setId: number; title: string };
  LearningSession: {
    sessionId: number;
    vocabSetId?: number;
    mode?: 'learning' | 'review';
  };
};

export type BattleStackParamList = {
  GymMap: undefined;
  ArenaBattle: { gymId?: number };
  PvpLobby: undefined;
  PvpBattle: { battleSessionId?: number };
};

export type PetsStackParamList = {
  PetsList: undefined;
  PetDetail: { petId: number };
};

export type ProfileStackParamList = {
  ProfileMain: undefined;
  Achievements: undefined;
  DailyQuests: undefined;
  Settings: undefined;
};

export type MainTabParamList = {
  HomeTab: undefined;
  LearnTab: NavigatorScreenParams<LearnStackParamList> | undefined;
  BattleTab: undefined;
  PetsTab: undefined;
  ProfileTab: undefined;
};

// ── Sub-Stacks ───────────────────────────────────────────────
const HomeStack = createNativeStackNavigator<HomeStackParamList>();
const HomeStackNavigator = () => (
  <HomeStack.Navigator screenOptions={{ headerShown: false }}>
    <HomeStack.Screen name="HomeMain" component={HomeScreen} />
    <HomeStack.Screen name="Leaderboard" component={LeaderboardScreen} />
  </HomeStack.Navigator>
);

const LearnStack = createNativeStackNavigator<LearnStackParamList>();
const LearnStackNavigator = () => (
  <LearnStack.Navigator screenOptions={{ headerShown: false }}>
    <LearnStack.Screen name="VocabSetList" component={VocabSetListScreen} />
    <LearnStack.Screen name="VocabSetDetail" component={VocabSetDetailScreen} />
    <LearnStack.Screen
      name="LearningSession"
      component={LearningSessionScreen}
      options={{ gestureEnabled: false }}
    />
  </LearnStack.Navigator>
);

const BattleStack = createNativeStackNavigator<BattleStackParamList>();
const BattleStackNavigator = () => (
  <BattleStack.Navigator screenOptions={{ headerShown: false }}>
    <BattleStack.Screen name="GymMap" component={GymMapScreen} />
    <BattleStack.Screen name="ArenaBattle" component={ArenaBattleScreen} />
    <BattleStack.Screen name="PvpLobby" component={PvpLobbyScreen} />
    <BattleStack.Screen
      name="PvpBattle"
      component={PvpBattleScreen}
      options={{ gestureEnabled: false }}
    />
  </BattleStack.Navigator>
);

const PetsStack = createNativeStackNavigator<PetsStackParamList>();
const PetsStackNavigator = () => (
  <PetsStack.Navigator screenOptions={{ headerShown: false }}>
    <PetsStack.Screen name="PetsList" component={PetsScreen} />
    <PetsStack.Screen name="PetDetail" component={PetDetailScreen} />
  </PetsStack.Navigator>
);

const ProfileStack = createNativeStackNavigator<ProfileStackParamList>();
const ProfileStackNavigator = () => (
  <ProfileStack.Navigator screenOptions={{ headerShown: false }}>
    <ProfileStack.Screen name="ProfileMain" component={ProfileScreen} />
    <ProfileStack.Screen name="Achievements" component={AchievementScreen} />
    <ProfileStack.Screen name="DailyQuests" component={DailyQuestScreen} />
    <ProfileStack.Screen name="Settings" component={SettingsScreen} />
  </ProfileStack.Navigator>
);

// ── Bottom Tabs ──────────────────────────────────────────────
const Tab = createBottomTabNavigator<MainTabParamList>();

export const MainTabs: React.FC = () => (
  <Tab.Navigator
    screenOptions={({ route }) => ({
      headerShown: false,
      tabBarActiveTintColor: '#3b82f6',
      tabBarInactiveTintColor: '#9ca3af',
      tabBarStyle: {
        backgroundColor: '#ffffff',
        borderTopWidth: 1,
        borderTopColor: '#e5e7eb',
        paddingBottom: Platform.OS === 'ios' ? 20 : 8,
        paddingTop: 8,
        height: Platform.OS === 'ios' ? 84 : 64,
      },
      tabBarLabelStyle: { fontSize: 11, fontWeight: '600' },
      tabBarIcon: ({ color, size, focused }) => {
        type IoniconsName = React.ComponentProps<typeof Ionicons>['name'];
        const icons: Record<string, [IoniconsName, IoniconsName]> = {
          HomeTab: ['home', 'home-outline'],
          LearnTab: ['book', 'book-outline'],
          BattleTab: ['flash', 'flash-outline'],
          PetsTab: ['paw', 'paw-outline'],
          ProfileTab: ['person', 'person-outline'],
        };
        const [active, inactive] = icons[route.name] ?? ['ellipse', 'ellipse-outline'];
        return <Ionicons name={focused ? active : inactive} size={size} color={color} />;
      },
    })}
  >
    <Tab.Screen name="HomeTab" component={HomeStackNavigator} options={{ tabBarLabel: 'Trang chủ' }} />
    <Tab.Screen name="LearnTab" component={LearnStackNavigator} options={{ tabBarLabel: 'Học' }} />
    <Tab.Screen name="BattleTab" component={BattleStackNavigator} options={{ tabBarLabel: 'Chiến đấu' }} />
    <Tab.Screen name="PetsTab" component={PetsStackNavigator} options={{ tabBarLabel: 'Pet' }} />
    <Tab.Screen name="ProfileTab" component={ProfileStackNavigator} options={{ tabBarLabel: 'Cá nhân' }} />
  </Tab.Navigator>
);
