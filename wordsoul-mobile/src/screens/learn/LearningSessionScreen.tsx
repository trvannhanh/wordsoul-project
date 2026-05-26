import React, { useCallback, useEffect, useRef, useState } from 'react';
import {
  View,
  Text,
  TouchableOpacity,
  Alert,
  ActivityIndicator,
  ScrollView,
  TextInput,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import Animated, {
  useSharedValue,
  useAnimatedStyle,
  withSpring,
  withSequence,
  withTiming,
  runOnJS,
} from 'react-native-reanimated';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { LearnStackParamList } from '../../navigation/MainTabs';
import {
  fetchQuizOfSession,
  answerQuiz,
  completeLearningSession,
} from '../../services/learningSession';
import type { QuizQuestionDto, AnswerResponseDto } from '../../types/LearningSessionDto';
import { QuestionTypeEnum } from '../../types/LearningSessionDto';
import { ProgressBar } from '../../components/ui/ProgressBar';
import { Ionicons } from '@expo/vector-icons';
import { createAudioPlayer } from 'expo-audio';

type Props = NativeStackScreenProps<LearnStackParamList, 'LearningSession'>;

type AnswerState = 'idle' | 'correct' | 'wrong';

export const LearningSessionScreen: React.FC<Props> = ({ navigation, route }) => {
  const { sessionId } = route.params;

  const [questions, setQuestions] = useState<QuizQuestionDto[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [loading, setLoading] = useState(true);
  const [answerState, setAnswerState] = useState<AnswerState>('idle');
  const [answerResponse, setAnswerResponse] = useState<AnswerResponseDto | null>(null);
  const [flipped, setFlipped] = useState(false);
  const [fillInput, setFillInput] = useState('');
  const [hintCount, setHintCount] = useState(0);
  const [completedCount, setCompletedCount] = useState(0);
  const [finishing, setFinishing] = useState(false);
  const startTime = useRef(Date.now());

  // Animation values
  const shakeX = useSharedValue(0);
  const scaleY = useSharedValue(1);
  const flipRotate = useSharedValue(0);

  useEffect(() => {
    fetchQuizOfSession(sessionId)
      .then((q) => setQuestions(q))
      .catch(() => Alert.alert('Lỗi', 'Không thể tải câu hỏi'))
      .finally(() => setLoading(false));
  }, [sessionId]);

  const currentQ = questions[currentIndex];
  const progress = questions.length > 0 ? (completedCount / questions.length) * 100 : 0;

  const playAudio = useCallback(async (url: string) => {
    try {
      const player = createAudioPlayer({ uri: url });
      player.play();
    } catch {
      // ignore audio errors
    }
  }, []);

  const shakeAnimation = () => {
    shakeX.value = withSequence(
      withTiming(-12, { duration: 60 }),
      withTiming(12, { duration: 60 }),
      withTiming(-8, { duration: 60 }),
      withTiming(8, { duration: 60 }),
      withTiming(0, { duration: 60 }),
    );
  };

  const bounceAnimation = () => {
    scaleY.value = withSequence(
      withSpring(1.08),
      withSpring(1),
    );
  };

  const shakeStyle = useAnimatedStyle(() => ({
    transform: [{ translateX: shakeX.value }],
  }));
  const bounceStyle = useAnimatedStyle(() => ({
    transform: [{ scale: scaleY.value }],
  }));

  const handleAnswer = useCallback(
    async (answer: string) => {
      if (answerState !== 'idle' || !currentQ) return;
      const elapsed = (Date.now() - startTime.current) / 1000;
      startTime.current = Date.now();

      try {
        const res = await answerQuiz(sessionId, {
          vocabularyId: currentQ.vocabularyId,
          questionType: currentQ.questionType,
          answer,
          responseTimeSeconds: Math.round(elapsed),
          hintCount,
        });

        setAnswerResponse(res);
        if (res.isCorrect) {
          setAnswerState('correct');
          bounceAnimation();
          if (currentQ.pronunciationUrl) playAudio(currentQ.pronunciationUrl);
          if (res.isVocabularyCompleted) {
            setCompletedCount((c) => c + 1);
          }
        } else {
          setAnswerState('wrong');
          shakeAnimation();
        }
      } catch {
        // silent
      }
    },
    [answerState, currentQ, sessionId, hintCount, playAudio],
  );

  const handleNext = useCallback(() => {
    setAnswerState('idle');
    setAnswerResponse(null);
    setFlipped(false);
    setFillInput('');
    setHintCount(0);
    startTime.current = Date.now();

    if (currentIndex + 1 >= questions.length) {
      // All questions done — complete session
      handleComplete();
    } else {
      setCurrentIndex((i) => i + 1);
    }
  }, [currentIndex, questions.length]);

  const handleComplete = useCallback(async () => {
    setFinishing(true);
    try {
      await completeLearningSession(sessionId);
      navigation.replace('VocabSetList');
    } catch {
      navigation.replace('VocabSetList');
    }
  }, [sessionId, navigation]);

  const handleExit = () => {
    Alert.alert(
      'Thoát phiên học?',
      'Tiến độ hiện tại sẽ được lưu lại.',
      [
        { text: 'Tiếp tục học', style: 'cancel' },
        { text: 'Thoát', style: 'destructive', onPress: () => navigation.goBack() },
      ],
    );
  };

  if (loading || !currentQ) {
    return (
      <SafeAreaView className="flex-1 bg-white dark:bg-gray-950 items-center justify-center">
        <ActivityIndicator size="large" color="#3b82f6" />
      </SafeAreaView>
    );
  }

  if (finishing) {
    return (
      <SafeAreaView className="flex-1 bg-white dark:bg-gray-950 items-center justify-center px-6">
        <Text className="text-2xl font-black text-gray-900 dark:text-white text-center">
          Hoàn thành!
        </Text>
        <Text className="text-gray-500 mt-2 text-center">
          Bạn đã học xong phiên này
        </Text>
        <ActivityIndicator size="small" color="#3b82f6" className="mt-6" />
      </SafeAreaView>
    );
  }

  const isFlashcard = currentQ.questionType === QuestionTypeEnum.Flashcard;
  const isListening = currentQ.questionType === QuestionTypeEnum.Listening;
  const isMultipleChoice = currentQ.questionType === QuestionTypeEnum.MultipleChoice;
  const isFillInBlank = currentQ.questionType === QuestionTypeEnum.FillInBlank;

  return (
    <SafeAreaView className="flex-1 bg-gray-50 dark:bg-gray-950">
      {/* Header */}
      <View className="flex-row items-center px-4 py-3">
        <TouchableOpacity onPress={handleExit}>
          <Ionicons name="close" size={24} color="#6b7280" />
        </TouchableOpacity>
        <View className="flex-1 mx-4">
          <ProgressBar value={progress} color="#3b82f6" height={8} />
        </View>
        <Text className="text-gray-600 dark:text-gray-400 text-sm font-medium">
          {completedCount}/{questions.length}
        </Text>
      </View>

      {/* Question type badge */}
      <View className="px-4 mb-3">
        <View className="self-start bg-blue-100 dark:bg-blue-900/40 px-3 py-1 rounded-full">
          <Text className="text-blue-600 dark:text-blue-300 text-xs font-bold">
            {isFlashcard
              ? 'Flashcard'
              : isListening
                ? 'Nghe'
                : isMultipleChoice
                  ? 'Trắc nghiệm'
                  : 'Điền vào chỗ trống'}
          </Text>
        </View>
      </View>

      <ScrollView
        contentContainerStyle={{ flexGrow: 1, paddingHorizontal: 16 }}
        keyboardShouldPersistTaps="handled"
        showsVerticalScrollIndicator={false}
      >
        {/* ── Flashcard ── */}
        {isFlashcard && (
          <TouchableOpacity
            className={`rounded-3xl p-8 items-center justify-center min-h-64 shadow-sm ${flipped
              ? 'bg-blue-600'
              : 'bg-white dark:bg-gray-800'
              }`}
            onPress={() => setFlipped((v) => !v)}
            activeOpacity={0.9}
          >
            {!flipped ? (
              <>
                <Text className="text-4xl font-black text-gray-900 dark:text-white text-center mb-2">
                  {currentQ.word}
                </Text>
                {currentQ.pronunciation && (
                  <Text className="text-gray-400 text-base">
                    /{currentQ.pronunciation}/
                  </Text>
                )}
                <Text className="text-gray-400 mt-4 text-sm">
                  Nhấn để xem nghĩa
                </Text>
              </>
            ) : (
              <>
                <Text className="text-2xl font-bold text-white text-center">
                  {currentQ.meaning}
                </Text>
                {currentQ.description && (
                  <Text className="text-blue-100 text-sm mt-3 text-center">
                    {currentQ.description}
                  </Text>
                )}
              </>
            )}
          </TouchableOpacity>
        )}

        {/* ── Listening ── */}
        {isListening && (
          <View className="bg-white dark:bg-gray-800 rounded-3xl p-6 items-center mb-4 shadow-sm">
            <TouchableOpacity
              className="w-20 h-20 bg-blue-100 dark:bg-blue-900/40 rounded-full items-center justify-center mb-4"
              onPress={() => {
                if (currentQ.pronunciationUrl) playAudio(currentQ.pronunciationUrl);
              }}
            >
              <Ionicons name="volume-high" size={36} color="#3b82f6" />
            </TouchableOpacity>
            <Text className="text-gray-500 dark:text-gray-400">
              Nhấn để nghe phát âm
            </Text>
          </View>
        )}

        {/* ── Multiple Choice & Listening options ── */}
        {(isMultipleChoice || isListening) && (
          <>
            {!isListening && (
              <View className="bg-white dark:bg-gray-800 rounded-3xl p-6 mb-4 shadow-sm items-center">
                <Text className="text-2xl font-black text-gray-900 dark:text-white">
                  {currentQ.questionPrompt ?? currentQ.meaning ?? currentQ.word}
                </Text>
              </View>
            )}
            <Animated.View style={shakeStyle} className="gap-y-3">
              {currentQ.options?.map((option, idx) => {
                let optionStyle = 'bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700';
                if (answerState !== 'idle') {
                  if (option === answerResponse?.correctAnswer) {
                    optionStyle = 'bg-green-50 border-2 border-green-400';
                  } else if (option !== answerResponse?.correctAnswer && answerState === 'wrong') {
                    optionStyle = 'bg-red-50 border-2 border-red-300';
                  }
                }
                return (
                  <Animated.View key={idx} style={answerState !== 'idle' && option === answerResponse?.correctAnswer ? bounceStyle : {}}>
                    <TouchableOpacity
                      className={`${optionStyle} rounded-2xl px-5 py-4`}
                      onPress={() => handleAnswer(option)}
                      disabled={answerState !== 'idle'}
                      activeOpacity={0.8}
                    >
                      <Text className="text-gray-800 dark:text-gray-200 text-base font-semibold text-center">
                        {option}
                      </Text>
                    </TouchableOpacity>
                  </Animated.View>
                );
              })}
            </Animated.View>
          </>
        )}

        {/* ── Fill in Blank ── */}
        {isFillInBlank && (
          <>
            <View className="bg-white dark:bg-gray-800 rounded-3xl p-6 mb-4 shadow-sm">
              <Text className="text-lg font-semibold text-gray-900 dark:text-white text-center leading-7">
                {currentQ.questionPrompt ?? currentQ.description ?? `_____ (${currentQ.meaning})`}
              </Text>
            </View>
            <Animated.View style={shakeStyle}>
              <TextInput
                className={`border-2 rounded-2xl px-5 py-4 text-base text-gray-900 dark:text-white bg-white dark:bg-gray-800 ${answerState === 'correct'
                  ? 'border-green-400'
                  : answerState === 'wrong'
                    ? 'border-red-400'
                    : 'border-gray-200 dark:border-gray-700'
                  }`}
                placeholder="Nhập từ..."
                placeholderTextColor="#9ca3af"
                value={fillInput}
                onChangeText={setFillInput}
                autoCapitalize="none"
                editable={answerState === 'idle'}
                returnKeyType="done"
                onSubmitEditing={() => handleAnswer(fillInput.trim())}
              />
            </Animated.View>
            {answerState === 'idle' && (
              <TouchableOpacity
                className="bg-blue-600 rounded-2xl py-4 mt-3 items-center"
                onPress={() => handleAnswer(fillInput.trim())}
              >
                <Text className="text-white font-bold text-base">Xác nhận</Text>
              </TouchableOpacity>
            )}
          </>
        )}

        {/* ── Flashcard Buttons ── */}
        {isFlashcard && flipped && answerState === 'idle' && (
          <View className="flex-row gap-x-3 mt-6">
            <TouchableOpacity
              className="flex-1 bg-red-100 dark:bg-red-900/30 border-2 border-red-300 rounded-2xl py-4 items-center"
              onPress={() => handleAnswer('__DONT_KNOW__')}
            >
              <Text className="text-red-600 font-bold">Chưa biết</Text>
            </TouchableOpacity>
            <TouchableOpacity
              className="flex-1 bg-green-100 dark:bg-green-900/30 border-2 border-green-300 rounded-2xl py-4 items-center"
              onPress={() => handleAnswer(currentQ.word ?? '')}
            >
              <Text className="text-green-600 font-bold">Đã biết</Text>
            </TouchableOpacity>
          </View>
        )}

        {/* ── Answer Feedback ── */}
        {answerState !== 'idle' && (
          <View
            className={`mt-4 rounded-2xl p-4 ${answerState === 'correct'
              ? 'bg-green-50 dark:bg-green-900/20 border border-green-200'
              : 'bg-red-50 dark:bg-red-900/20 border border-red-200'
              }`}
          >
            <Text
              className={`font-bold text-base mb-1 ${answerState === 'correct' ? 'text-green-700' : 'text-red-600'
                }`}
            >
              {answerState === 'correct' ? 'Chính xác!' : 'Sai rồi!'}
            </Text>
            {answerState === 'wrong' && (
              <Text className="text-gray-600 dark:text-gray-400 text-sm">
                Đáp án đúng:{' '}
                <Text className="font-bold text-gray-800 dark:text-white">
                  {answerResponse?.correctAnswer}
                </Text>
              </Text>
            )}
            <TouchableOpacity
              className={`mt-3 rounded-xl py-3 items-center ${answerState === 'correct' ? 'bg-green-500' : 'bg-blue-600'
                }`}
              onPress={handleNext}
            >
              <Text className="text-white font-bold">
                {currentIndex + 1 >= questions.length ? 'Hoàn thành' : 'Tiếp theo'}
              </Text>
            </TouchableOpacity>
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
};
