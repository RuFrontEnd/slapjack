"use client";

import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Zap } from 'lucide-react';
import styles from './page.module.css';

interface Card {
  id: string; value: string; suit: string; color: 'red' | 'black'; rotate: number;
}

interface SlapAction {
  id: string; playerName: string; emoji: string; rotate: number; x: number; y: number;
}

export default function Home() {
  const [cardsOnTable, setCardsOnTable] = useState<Card[]>([]);
  const [slaps, setSlaps] = useState<SlapAction[]>([]);
  const [isSlapped, setIsSlapped] = useState(false);

  const opponents = [
    { name: 'Foxie', pos: styles.playerTop, avatar: '🦊', emoji: '🖐️' },
    { name: 'Katt', pos: styles.playerLeft, avatar: '🐱', emoji: '🐾' },
    { name: 'Doggo', pos: styles.playerRight, avatar: '🐶', emoji: '✋' },
  ];

  // 翻牌邏輯
  const flipCard = () => {
    const suits = ['♠', '♥', '♦', '♣'];
    const values = ['A', '2', '3', '4', '5', '6', '7', '8', '9', '10', 'J', 'Q', 'K'];
    const suit = suits[Math.floor(Math.random() * suits.length)];
    const newCard: Card = {
      id: Math.random().toString(36),
      value: values[Math.floor(Math.random() * values.length)],
      suit,
      color: (suit === '♥' || suit === '♦') ? 'red' : 'black',
      rotate: Math.floor(Math.random() * 20) - 10,
    };
    setCardsOnTable(prev => [...prev, newCard]);
    setSlaps([]); // 新的一輪清空手掌
    setIsSlapped(false);
  };

  // 模擬不同玩家拍牌
  const triggerSlap = (playerName: string, emoji: string) => {
    const newSlap: SlapAction = {
      id: Math.random().toString(36),
      playerName,
      emoji,
      rotate: Math.floor(Math.random() * 60) - 30,
      x: Math.floor(Math.random() * 40) - 20,
      y: Math.floor(Math.random() * 40) - 20,
    };
    setSlaps(prev => [...prev, newSlap]);
  };

  const handleMySlap = () => {
    if (isSlapped) return;
    setIsSlapped(true);
    triggerSlap("You", "✋");
    
    // 模擬 0.2 秒後對手也拍了 (這之後會由 SignalR 傳來)
    setTimeout(() => triggerSlap("Foxie", "🖐️"), 150);
    setTimeout(() => triggerSlap("Doggo", "✋"), 300);
  };

  return (
    <div className={styles.gameContainer}>
      {/* 渲染對手 (省略重複部分，與之前相同) */}
      {opponents.map((op, i) => (
        <div key={i} className={`${styles.playerSlot} ${op.pos}`}>
          <div className={styles.avatar}>{op.avatar}</div>
          <p className="font-bold">{op.name}</p>
        </div>
      ))}

      {/* 中央：牌堆戰場 */}
      <div className={styles.tableCenter}>
        <div className={styles.deckWrapper}>
          {/* 渲染卡牌 */}
          <AnimatePresence>
            {cardsOnTable.map((card, index) => (
              <motion.div
                key={card.id}
                initial={{ y: -400, opacity: 0, rotate: card.rotate + 20 }}
                animate={{ y: 0, opacity: 1, rotate: card.rotate }}
                className={styles.playingCard}
                style={{ zIndex: index, color: card.color === 'red' ? '#dc2626' : '#1e293b' }}
              >
                <div className={styles.cardTop}>{card.value}<br/>{card.suit}</div>
                <div className={styles.cardSuit}>{card.suit}</div>
                <div className={styles.cardBottom}>{card.value}<br/>{card.suit}</div>
              </motion.div>
            ))}
          </AnimatePresence>

          {/* 渲染拍在上面的手掌 */}
          <div className={styles.handContainer}>
            <AnimatePresence>
              {slaps.map((slap, index) => (
                <motion.div
                  key={slap.id}
                  initial={{ scale: 3, opacity: 0 }}
                  animate={{ scale: 1, opacity: 1, x: slap.x, y: slap.y, rotate: slap.rotate }}
                  className={styles.slapHand}
                  style={{ zIndex: 1000 + index }}
                >
                  <span>{slap.emoji}</span>
                  <div className={styles.handLabel}>{slap.playerName}</div>
                </motion.div>
              ))}
            </AnimatePresence>
          </div>
        </div>
      </div>

      {/* 底部控制 */}
      <div className={`${styles.playerSlot} ${styles.playerBottom}`}>
        <div className={styles.controls}>
          <div className="flex gap-4 mb-4">
            <button onClick={flipCard} className="px-4 py-2 bg-white/10 rounded">Next Card</button>
            <button onClick={() => triggerSlap("Foxie", "🖐️")} className="px-4 py-2 bg-white/10 rounded text-xs">Simulate Opponent Slap</button>
          </div>
          
          <button 
            className={styles.slapBtn}
            disabled={cardsOnTable.length === 0}
            onClick={handleMySlap}
          >
            SLAP!
          </button>
        </div>
      </div>
    </div>
  );
}