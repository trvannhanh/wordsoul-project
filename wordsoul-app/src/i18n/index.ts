import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

// Import Vietnamese translation resources
import viCommon from './locales/vi/common.json';
import viHeader from './locales/vi/header.json';
import viAuth from './locales/vi/auth.json';
import viVocabulary from './locales/vi/vocabulary.json';
import viPets from './locales/vi/pets.json';
import viGym from './locales/vi/gym.json';
import viPvp from './locales/vi/pvp.json';
import viCommunity from './locales/vi/community.json';

// Import English translation resources
import enCommon from './locales/en/common.json';
import enHeader from './locales/en/header.json';
import enAuth from './locales/en/auth.json';
import enVocabulary from './locales/en/vocabulary.json';
import enPets from './locales/en/pets.json';
import enGym from './locales/en/gym.json';
import enPvp from './locales/en/pvp.json';
import enCommunity from './locales/en/community.json';

export const resources = {
  vi: {
    common: viCommon,
    header: viHeader,
    auth: viAuth,
    vocabulary: viVocabulary,
    pets: viPets,
    gym: viGym,
    pvp: viPvp,
    community: viCommunity,
  },
  en: {
    common: enCommon,
    header: enHeader,
    auth: enAuth,
    vocabulary: enVocabulary,
    pets: enPets,
    gym: enGym,
    pvp: enPvp,
    community: enCommunity,
  },
} as const;

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    fallbackLng: 'vi',
    defaultNS: 'common',
    detection: {
      order: ['localStorage', 'navigator'],
      lookupLocalStorage: 'wordsoul_lang',
      caches: ['localStorage'],
    },
    interpolation: {
      escapeValue: false, // React already escapes values to prevent XSS
    },
  });

export default i18n;
