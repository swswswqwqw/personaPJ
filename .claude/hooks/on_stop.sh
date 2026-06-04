#!/bin/bash
TODAY=$(date +%Y-%m-%d)
CLAUDE_MD="docs/CLAUDE.md"
LOG_FILE=".claude/logs/session_$(date +%Y%m%d_%H%M%S).log"

mkdir -p .claude/logs

echo "=== Session Stop: $(date) ===" >> "$LOG_FILE"

if [ -f "$CLAUDE_MD" ]; then
  if grep -q "$TODAY" "$CLAUDE_MD"; then
    echo "[OK] CLAUDE.md は今日の日付で更新済み" >> "$LOG_FILE"
  else
    echo "[WARN] CLAUDE.md が今日の日付で更新されていません" >> "$LOG_FILE"
    echo "[WARN] routine_prompt.md の STEP 7 が完了しているか確認してください" >> "$LOG_FILE"
  fi
else
  echo "[WARN] docs/CLAUDE.md が見つかりません" >> "$LOG_FILE"
fi
