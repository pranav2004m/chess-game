#!/bin/bash
# Chess Server Testing Script
# Run this to verify the server is working correctly

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

SERVER_URL="http://localhost:5000"

echo -e "${YELLOW}🎮 Chess Server Testing Script${NC}"
echo -e "${YELLOW}================================${NC}\n"

# Test 1: Health Check
echo -e "${YELLOW}Test 1: Health Check${NC}"
echo "GET /health"
HEALTH=$(curl -s "$SERVER_URL/health")
echo "Response: $HEALTH"
if [[ $HEALTH == *"healthy"* ]]; then
    echo -e "${GREEN}✅ PASSED${NC}\n"
else
    echo -e "${RED}❌ FAILED${NC}\n"
    exit 1
fi

# Test 2: Create Game
echo -e "${YELLOW}Test 2: Create Game${NC}"
echo "POST /game/create"
CREATE_RESPONSE=$(curl -s -X POST "$SERVER_URL/game/create")
echo "Response: $CREATE_RESPONSE"
GAME_ID=$(echo $CREATE_RESPONSE | grep -o '"game_id":"[^"]*"' | cut -d'"' -f4)
echo "Game ID: $GAME_ID"
if [[ ! -z "$GAME_ID" ]]; then
    echo -e "${GREEN}✅ PASSED${NC}\n"
else
    echo -e "${RED}❌ FAILED${NC}\n"
    exit 1
fi

# Test 3: Join as White
echo -e "${YELLOW}Test 3: Join Game as White${NC}"
echo "POST /game/$GAME_ID/join"
JOIN_WHITE=$(curl -s -X POST "$SERVER_URL/game/$GAME_ID/join" \
  -H "Content-Type: application/json" \
  -d '{"player_id":"player1","color":"white"}')
echo "Response: $JOIN_WHITE"
if [[ $JOIN_WHITE == *"waiting"* ]]; then
    echo -e "${GREEN}✅ PASSED${NC}\n"
else
    echo -e "${RED}❌ FAILED${NC}\n"
    exit 1
fi

# Test 4: Join as Black
echo -e "${YELLOW}Test 4: Join Game as Black${NC}"
echo "POST /game/$GAME_ID/join"
JOIN_BLACK=$(curl -s -X POST "$SERVER_URL/game/$GAME_ID/join" \
  -H "Content-Type: application/json" \
  -d '{"player_id":"player2","color":"black"}')
echo "Response: $JOIN_BLACK"
if [[ $JOIN_BLACK == *"active"* ]]; then
    echo -e "${GREEN}✅ PASSED${NC}\n"
else
    echo -e "${RED}❌ FAILED${NC}\n"
    exit 1
fi

# Test 5: Make Move (White)
echo -e "${YELLOW}Test 5: Make Move (e2→e4)${NC}"
echo "POST /game/$GAME_ID/move"
MOVE=$(curl -s -X POST "$SERVER_URL/game/$GAME_ID/move" \
  -H "Content-Type: application/json" \
  -d '{"player_id":"player1","from":"e2","to":"e4","promotion":null}')
echo "Response: $MOVE"
if [[ $MOVE == *"black"* ]]; then
    echo -e "${GREEN}✅ PASSED${NC}\n"
else
    echo -e "${RED}❌ FAILED${NC}\n"
    exit 1
fi

# Test 6: Get Game State
echo -e "${YELLOW}Test 6: Get Game State${NC}"
echo "GET /game/$GAME_ID/state"
STATE=$(curl -s "$SERVER_URL/game/$GAME_ID/state")
echo "Response: $STATE"
MOVE_COUNT=$(echo $STATE | grep -o '"move_count":[0-9]*' | cut -d':' -f2)
echo "Move Count: $MOVE_COUNT"
if [[ $MOVE_COUNT == "1" ]]; then
    echo -e "${GREEN}✅ PASSED${NC}\n"
else
    echo -e "${RED}❌ FAILED${NC}\n"
    exit 1
fi

# Test 7: Make Move (Black)
echo -e "${YELLOW}Test 7: Make Move (e7→e5)${NC}"
echo "POST /game/$GAME_ID/move"
MOVE2=$(curl -s -X POST "$SERVER_URL/game/$GAME_ID/move" \
  -H "Content-Type: application/json" \
  -d '{"player_id":"player2","from":"e7","to":"e5","promotion":null}')
echo "Response: $MOVE2"
if [[ $MOVE2 == *"white"* ]]; then
    echo -e "${GREEN}✅ PASSED${NC}\n"
else
    echo -e "${RED}❌ FAILED${NC}\n"
    exit 1
fi

# Test 8: Try Invalid Move (Not Your Turn)
echo -e "${YELLOW}Test 8: Try Invalid Move (Not Your Turn)${NC}"
echo "POST /game/$GAME_ID/move (should fail)"
INVALID=$(curl -s -X POST "$SERVER_URL/game/$GAME_ID/move" \
  -H "Content-Type: application/json" \
  -d '{"player_id":"player2","from":"e5","to":"e4","promotion":null}')
echo "Response: $INVALID"
if [[ $INVALID == *"false"* ]]; then
    echo -e "${GREEN}✅ PASSED${NC}\n"
else
    echo -e "${RED}❌ FAILED${NC}\n"
    exit 1
fi

# Test 9: End Game
echo -e "${YELLOW}Test 9: End Game${NC}"
echo "POST /game/$GAME_ID/end"
END=$(curl -s -X POST "$SERVER_URL/game/$GAME_ID/end" \
  -H "Content-Type: application/json" \
  -d '{"winner":"white","reason":"checkmate"}')
echo "Response: $END"
if [[ $END == *"finished"* ]]; then
    echo -e "${GREEN}✅ PASSED${NC}\n"
else
    echo -e "${RED}❌ FAILED${NC}\n"
    exit 1
fi

# Test 10: Delete Game
echo -e "${YELLOW}Test 10: Delete Game${NC}"
echo "DELETE /game/$GAME_ID/delete"
DELETE=$(curl -s -X DELETE "$SERVER_URL/game/$GAME_ID/delete")
echo "Response: $DELETE"
if [[ $DELETE == *"true"* ]]; then
    echo -e "${GREEN}✅ PASSED${NC}\n"
else
    echo -e "${RED}❌ FAILED${NC}\n"
    exit 1
fi

# Test 11: Verify Game is Deleted
echo -e "${YELLOW}Test 11: Verify Game Deleted${NC}"
echo "GET /game/$GAME_ID/state (should fail)"
DELETED=$(curl -s "$SERVER_URL/game/$GAME_ID/state")
echo "Response: $DELETED"
if [[ $DELETED == *"false"* ]]; then
    echo -e "${GREEN}✅ PASSED${NC}\n"
else
    echo -e "${RED}❌ FAILED${NC}\n"
    exit 1
fi

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}✅ All Tests PASSED!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo "Server is working correctly!"
