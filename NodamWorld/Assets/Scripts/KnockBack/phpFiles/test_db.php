<?php
// DB 연결 테스트
define('DB_HOST', 'localhost');
define('DB_NAME', 'knockback');
define('DB_USER', 'knockback');  // ← 수정
define('DB_PASS', 'ansdustn1!');

header('Content-Type: application/json; charset=utf-8');

try {
    $pdo = new PDO(
        "mysql:host=" . DB_HOST . ";dbname=" . DB_NAME . ";charset=utf8mb4",
        DB_USER,
        DB_PASS,
        [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]
    );
    
    // 테이블 존재 확인
    $stmt = $pdo->query("SHOW TABLES LIKE 'tower_users'");
    $exists = $stmt->fetch();
    
    if ($exists) {
        // 테이블 내용도 확인
        $stmt = $pdo->query("SELECT COUNT(*) as count FROM tower_users");
        $result = $stmt->fetch();
        
        echo json_encode([
            'success' => true,
            'message' => 'DB 연결 성공 및 tower_users 테이블 존재',
            'table_exists' => true,
            'user_count' => $result['count'],
            'database' => DB_NAME,
            'user' => DB_USER
        ]);
    } else {
        echo json_encode([
            'success' => true,
            'message' => 'DB 연결 성공하나 tower_users 테이블 없음',
            'table_exists' => false,
            'database' => DB_NAME,
            'user' => DB_USER
        ]);
    }
    
} catch (PDOException $e) {
    echo json_encode([
        'success' => false,
        'message' => 'DB 연결 실패: ' . $e->getMessage(),
        'error_code' => $e->getCode(),
        'tried_user' => DB_USER,
        'tried_database' => DB_NAME
    ]);
}
?>