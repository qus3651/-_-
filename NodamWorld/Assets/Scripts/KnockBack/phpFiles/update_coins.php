<?php
require_once 'db_config.php';

// POST 데이터 받기
$userId = isset($_POST['user_id']) ? (int)$_POST['user_id'] : 0;
$earnedCoins = isset($_POST['earned_coins']) ? (int)$_POST['earned_coins'] : 0;

// 입력 검증
if ($userId <= 0) {
    echo json_encode([
        'success' => false,
        'message' => '유효하지 않은 유저 ID입니다.'
    ]);
    exit;
}

if ($earnedCoins < 0) {
    echo json_encode([
        'success' => false,
        'message' => '유효하지 않은 코인 값입니다.'
    ]);
    exit;
}

try {
    $pdo = getDBConnection();
    
    if (!$pdo) {
        throw new Exception('데이터베이스 연결 실패');
    }

    // 코인 업데이트
    $stmt = $pdo->prepare("UPDATE tower_users SET total_coins = total_coins + ? WHERE user_id = ?");
    $stmt->execute([$earnedCoins, $userId]);

    // 업데이트된 코인 가져오기
    $stmt = $pdo->prepare("SELECT total_coins FROM tower_users WHERE user_id = ?");
    $stmt->execute([$userId]);
    $user = $stmt->fetch();

    if ($user) {
        echo json_encode([
            'success' => true,
            'totalCoins' => (int)$user['total_coins'],
            'message' => '코인이 업데이트되었습니다.'
        ]);
    } else {
        throw new Exception('유저를 찾을 수 없습니다.');
    }

} catch (Exception $e) {
    error_log("UpdateCoins Error: " . $e->getMessage());
    echo json_encode([
        'success' => false,
        'message' => '코인 업데이트에 실패했습니다.'
    ]);
}
?>