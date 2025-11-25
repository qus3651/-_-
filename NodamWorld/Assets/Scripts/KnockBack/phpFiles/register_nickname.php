<?php
require_once 'db_config.php';

// POST 데이터 받기
$hash = isset($_POST['hash']) ? trim($_POST['hash']) : '';
$nickname = isset($_POST['nickname']) ? trim($_POST['nickname']) : '';

// 입력 검증
if (empty($hash) || empty($nickname)) {
    echo json_encode([
        'success' => false,
        'message' => '해시와 닉네임이 필요합니다.'
    ]);
    exit;
}

// 닉네임 길이 검증
if (mb_strlen($nickname) < 2 || mb_strlen($nickname) > 10) {
    echo json_encode([
        'success' => false,
        'message' => '닉네임은 2~10자여야 합니다.'
    ]);
    exit;
}

try {
    $pdo = getDBConnection();
    
    if (!$pdo) {
        throw new Exception('데이터베이스 연결 실패');
    }

    // 닉네임 중복 확인
    $stmt = $pdo->prepare("SELECT user_id FROM tower_users WHERE nickname = ?");
    $stmt->execute([$nickname]);
    
    if ($stmt->fetch()) {
        echo json_encode([
            'success' => false,
            'message' => '이미 사용 중인 닉네임입니다.'
        ]);
        exit;
    }

    // 유저 생성
    $stmt = $pdo->prepare("INSERT INTO tower_users (user_hash, nickname, total_coins) VALUES (?, ?, 0)");
    $stmt->execute([$hash, $nickname]);
    
    $userId = $pdo->lastInsertId();

    echo json_encode([
        'success' => true,
        'userId' => (int)$userId,
        'nickname' => $nickname,
        'message' => '닉네임이 등록되었습니다.'
    ]);

} catch (Exception $e) {
    error_log("RegisterNickname Error: " . $e->getMessage());
    echo json_encode([
        'success' => false,
        'message' => '닉네임 등록에 실패했습니다.'
    ]);
}
?>