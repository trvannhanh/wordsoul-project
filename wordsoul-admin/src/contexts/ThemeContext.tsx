'use client';

import React, { createContext, useContext, useEffect, useState } from 'react';
import { theme as antTheme } from 'antd';
import type { ThemeConfig } from 'antd';

type ColorScheme = 'light' | 'dark';

interface ThemeContextType {
  colorScheme: ColorScheme;
  toggleColorScheme: () => void;
  antdTheme: ThemeConfig;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

const baseTokens = {
  borderRadius: 8,
  fontFamily: 'Inter, system-ui, -apple-system, sans-serif',
  fontSize: 14,
};

const lightTokens: ThemeConfig = {
  algorithm: antTheme.defaultAlgorithm,
  token: {
    ...baseTokens,
    colorPrimary: '#4F46E5',
    colorBgBase: '#F8FAFC',
    colorBgContainer: '#FFFFFF',
    colorBgElevated: '#FFFFFF',
    colorBgLayout: '#F8FAFC',
    colorTextBase: '#0F172A',
    colorTextSecondary: '#475569',
    colorTextTertiary: '#94A3B8',
    colorBorder: '#E2E8F0',
    colorBorderSecondary: '#E2E8F0',
    colorSplit: '#E2E8F0',
    boxShadow: '0 1px 2px 0 rgb(0 0 0 / 0.05)',
    boxShadowSecondary: '0 4px 6px -1px rgb(0 0 0 / 0.07)',
  },
  components: {
    Menu: {
      itemBg: 'transparent',
      itemSelectedBg: '#EEF2FF',
      itemSelectedColor: '#4F46E5',
      itemHoverBg: '#F1F5F9',
      itemHoverColor: '#0F172A',
      itemColor: '#475569',
      subMenuItemBg: 'transparent',
    },
    Layout: {
      siderBg: '#FFFFFF',
      headerBg: '#FFFFFF',
      bodyBg: '#F8FAFC',
    },
    Card: {
      headerBg: 'transparent',
    },
    Table: {
      headerBg: '#F1F5F9',
      headerColor: '#475569',
      rowHoverBg: '#F8FAFC',
      borderColor: '#E2E8F0',
    },
    Button: {
      defaultBorderColor: '#E2E8F0',
      defaultColor: '#475569',
    },
    Input: {
      activeBorderColor: '#4F46E5',
      hoverBorderColor: '#A5B4FC',
    },
    Select: {
      optionSelectedBg: '#EEF2FF',
    },
    Tabs: {
      inkBarColor: '#4F46E5',
      itemSelectedColor: '#4F46E5',
    },
    Steps: {
      colorPrimary: '#4F46E5',
    },
    Tag: {
      defaultBg: '#F1F5F9',
      defaultColor: '#475569',
    },
    Modal: {
      headerBg: '#FFFFFF',
    },
    Statistic: {
      titleFontSize: 12,
    },
  },
};

const darkTokens: ThemeConfig = {
  algorithm: antTheme.darkAlgorithm,
  token: {
    ...baseTokens,
    colorPrimary: '#6366F1',
    colorBgBase: '#0C0E14',
    colorBgContainer: '#141620',
    colorBgElevated: '#1C1F2E',
    colorBgLayout: '#0C0E14',
    colorTextBase: '#F1F5F9',
    colorTextSecondary: '#94A3B8',
    colorTextTertiary: '#475569',
    colorBorder: '#1E2136',
    colorBorderSecondary: '#1E2136',
    colorSplit: '#1E2136',
    boxShadow: '0 1px 3px 0 rgb(0 0 0 / 0.3)',
    boxShadowSecondary: '0 4px 6px -1px rgb(0 0 0 / 0.4)',
  },
  components: {
    Menu: {
      itemBg: 'transparent',
      itemSelectedBg: '#1E1B4B',
      itemSelectedColor: '#818CF8',
      itemHoverBg: '#1C1F2E',
      itemHoverColor: '#F1F5F9',
      itemColor: '#94A3B8',
      subMenuItemBg: 'transparent',
    },
    Layout: {
      siderBg: '#141620',
      headerBg: '#141620',
      bodyBg: '#0C0E14',
    },
    Card: {
      headerBg: 'transparent',
    },
    Table: {
      headerBg: '#1C1F2E',
      headerColor: '#94A3B8',
      rowHoverBg: '#1C1F2E',
      borderColor: '#1E2136',
    },
    Button: {
      defaultBorderColor: '#2A2D48',
      defaultColor: '#94A3B8',
    },
    Input: {
      activeBorderColor: '#6366F1',
      hoverBorderColor: '#4338CA',
    },
    Select: {
      optionSelectedBg: '#1E1B4B',
    },
    Tabs: {
      inkBarColor: '#6366F1',
      itemSelectedColor: '#818CF8',
    },
    Steps: {
      colorPrimary: '#6366F1',
    },
    Tag: {
      defaultBg: '#1C1F2E',
      defaultColor: '#94A3B8',
    },
    Modal: {
      headerBg: '#141620',
    },
    Statistic: {
      titleFontSize: 12,
    },
  },
};

const STORAGE_KEY = 'vocamon-admin-theme';

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [colorScheme, setColorScheme] = useState<ColorScheme>('light');

  // Restore persisted preference on mount
  useEffect(() => {
    const saved = localStorage.getItem(STORAGE_KEY) as ColorScheme | null;
    if (saved === 'dark' || saved === 'light') {
      setColorScheme(saved);
    }
  }, []);

  // Apply data-theme attribute to <html> for CSS variable switching
  useEffect(() => {
    document.documentElement.setAttribute('data-theme', colorScheme);
    localStorage.setItem(STORAGE_KEY, colorScheme);
  }, [colorScheme]);

  const toggleColorScheme = () => {
    setColorScheme(prev => (prev === 'light' ? 'dark' : 'light'));
  };

  const antdTheme = colorScheme === 'dark' ? darkTokens : lightTokens;

  return (
    <ThemeContext.Provider value={{ colorScheme, toggleColorScheme, antdTheme }}>
      {children}
    </ThemeContext.Provider>
  );
}

export function useTheme() {
  const ctx = useContext(ThemeContext);
  if (!ctx) throw new Error('useTheme must be used within ThemeProvider');
  return ctx;
}
