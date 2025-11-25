<?php
require_once 'db_config.php';

// GET 데이터 받기
$userId = isset($_GET['user_id']) ? (int)$_GET['user_id'] : 0;

// 입력 검증
if ($userId <= 0) {
    echo json_encode([
        'success' => false,
        'message' => '유효하지 않은 유저 ID입니다.'
    ]);
    exit;
}

try {
    $pdo = getDBConnection();
    
    if (!$pdo) {
        throw new Exception('데이터베이스 연결 실패');
    }

    // 유저 데이터 가져오기
    $stmt = $pdo->prepare("SELECT nickname, total_coins FROM tower_users WHERE user_id = ?");
    $stmt->execute([$userId]);
    $user = $stmt->fetch();

    if ($user) {
        echo json_encode([
            'success' => true,
            'nickname' => $user['nickname'],
            'totalCoins' => (int)$user['total_coins']
        ]);
    } else {
        echo json_encode([
            'success' => false,
            'message' => '유저를 찾을 수 없습니다.'
        ]);
    }

} catch (Exception $e) {
    error_log("GetUserData Error: " . $e->getMessage());
    echo json_encode([
        'success' => false,
        'message' => '데이터 조회에 실패했습니다.'
    ]);
}
?>