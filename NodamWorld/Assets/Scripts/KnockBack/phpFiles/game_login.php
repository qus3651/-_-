<?php
require_once 'db_config.php';

// 디버깅 로그
error_log("=== game_login.php START ===");
error_log("POST data: " . print_r($_POST, true));

// POST 데이터 받기
$hash = isset($_POST['hash']) ? trim($_POST['hash']) : '';

error_log("Received hash: " . $hash);

// 해시 검증
if (empty($hash)) {
    error_log("Empty hash received");
    echo json_encode([
        'success' => false,
        'message' => '해시 값이 필요합니다.'
    ]);
    exit;
}

try {
    $pdo = getDBConnection();
    
    if (!$pdo) {
        throw new Exception('데이터베이스 연결 실패');
    }

    error_log("DB connection successful, searching for hash: " . $hash);

    // 해시로 유저 검색
    $stmt = $pdo->prepare("SELECT user_id, nickname, total_coins FROM tower_users WHERE user_hash = ?");
    $stmt->execute([$hash]);
    $user = $stmt->fetch();

    error_log("User search result: " . print_r($user, true));

    if ($user) {
        // 기존 유저
        $response = [
            'success' => true,
            'needNickname' => false,
            'userId' => (int)$user['user_id'],
            'nickname' => $user['nickname'],
            'totalCoins' => (int)$user['total_coins']
        ];
        error_log("Returning existing user: " . json_encode($response));
        echo json_encode($response);
    } else {
        // 신규 유저 - 닉네임 필요
        $response = [
            'success' => true,
            'needNickname' => true,
            'message' => '닉네임 등록이 필요합니다.'
        ];
        error_log("New user - need nickname");
        echo json_encode($response);
    }

} catch (Exception $e) {
    error_log("GameLogin Error: " . $e->getMessage());
    error_log("Stack trace: " . $e->getTraceAsString());
    echo json_encode([
        'success' => false,
        'message' => '서버 오류: ' . $e->getMessage()
    ]);
}

error_log("=== game_login.php END ===");
?>