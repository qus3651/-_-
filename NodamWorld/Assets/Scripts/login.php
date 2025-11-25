<?php
// login.php
$DB_HOST = "localhost";
$DB_USER = "nodamcoin";
$DB_PASS = "ansdustn1!";
$DB_NAME = "nodamcoin";

header('Content-Type: application/json; charset=UTF-8');

$userid   = isset($_POST['userid']) ? trim($_POST['userid']) : '';
$password = isset($_POST['password']) ? trim($_POST['password']) : '';

if ($userid === '' || $password === '') {
    echo json_encode(['ok' => false, 'error' => '아이디 또는 비밀번호가 입력되지 않았습니다.']);
    exit;
}

$conn = new mysqli($DB_HOST, $DB_USER, $DB_PASS, $DB_NAME);
if ($conn->connect_error) {
    echo json_encode(['ok' => false, 'error' => 'DB 연결 실패']);
    exit;
}
$conn->set_charset("utf8mb4");

$stmt = $conn->prepare("SELECT userid, name, wallet, gamemoney FROM users WHERE userid=? AND password=?");
$stmt->bind_param("ss", $userid, $password);
$stmt->execute();
$res = $stmt->get_result();

if ($row = $res->fetch_assoc()) {
    // 로그인 성공 → login_logs 테이블에 기록
    $ip = $_SERVER['REMOTE_ADDR'] ?? '';
    $ua = $_SERVER['HTTP_USER_AGENT'] ?? '';

    $log = $conn->prepare("INSERT INTO login_logs (userid, ip_address, user_agent) VALUES (?,?,?)");
    $log->bind_param("sss", $row['userid'], $ip, $ua);
    $log->execute();
    $log->close();

    echo json_encode([
        'ok' => true,
        'userid' => $row['userid'],
        'name' => $row['name'],
        'wallet' => $row['wallet'],
        'gamemoney' => intval($row['gamemoney'])
    ]);
} else {
    echo json_encode(['ok' => false, 'error' => '아이디 또는 비밀번호가 올바르지 않습니다.']);
}

$stmt->close();
$conn->close();
