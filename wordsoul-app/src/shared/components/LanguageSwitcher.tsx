import React from 'react';
import { useTranslation } from 'react-i18next';

export const LanguageSwitcher: React.FC<{ className?: string }> = ({ className = '' }) => {
  const { i18n } = useTranslation();
  const currentLang = i18n.language || 'vi';

  const toggleLanguage = () => {
    const nextLang = currentLang.startsWith('vi') ? 'en' : 'vi';
    i18n.changeLanguage(nextLang);
  };

  const isVi = currentLang.startsWith('vi');

  return (
    <button
      onClick={toggleLanguage}
      className={`flex items-center gap-1.5 px-2.5 py-1 text-xs font-pixel font-semibold rounded border border-gray-300 dark:border-gray-700 bg-gray-100 dark:bg-gray-800 text-gray-800 dark:text-gray-200 hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors custom-cursor ${className}`}
      title={isVi ? 'Switch to English' : 'Chuyển sang Tiếng Việt'}
      type="button"
    >
      <span className="text-sm">{isVi ? '🇻🇳' : '🇬🇧'}</span>
      <span>{isVi ? 'VI' : 'EN'}</span>
    </button>
  );
};

export default LanguageSwitcher;
