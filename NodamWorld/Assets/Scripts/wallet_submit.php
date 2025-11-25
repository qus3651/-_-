<?php
// wallet_submit.php
// 입력받은 지갑 주소를 지정 메일로 발송하고 JSON으로 결과를 반환
// PHP 7.x/8.x, Cafe24 APM 환경 가정

// 한글 메일 설정
mb_language("uni");
mb_internal_encoding("UTF-8");

// 응답 헤더
header('Content-Type: application/json; charset=UTF-8');

// 수신자 메일
$TO = 'sodowe@naver.com';

// 발신자 메일 (본인 도메인 주소로 변경 권장)
$FROM = 'noreply@yourdomain.com';

// 입력 값 취득
$wallet   = isset($_POST['wallet']) ? trim($_POST['wallet']) : '';
$collected = isset($_POST['collected']) ? intval($_POST['collected']) : 0;

// 간단 검증
if ($wallet === '' || mb_strlen($wallet) < 5) {
    echo json_encode(['ok' => false, 'error' => 'INVALID_WALLET']);
    exit;
}

// 메타 정보
$ip = isset($_SERVER['REMOTE_ADDR']) ? $_SERVER['REMOTE_ADDR'] : 'UNKNOWN';
$ua = isset($_SERVER['HTTP_USER_AGENT']) ? $_SERVER['HTTP_USER_AGENT'] : 'UNKNOWN';

// 시간대: KST
date_default_timezone_set('Asia/Seoul');
$now = date('Y-m-d H:i:s');

// 메일 제목/본문
$subject = 'NODAM 지갑 주소 제출';
$body = "다음과 같이 지갑 주소가 제출되었습니다.\n\n"
      . "지갑 주소: {$wallet}\n"
      . "수집 개수: {$collected}\n"
      . "제출 시각: {$now} (KST)\n"
      . "요청 IP : {$ip}\n"
      . "UserAgent: {$ua}\n";

// 헤더
$headers  = "MIME-Version: 1.0\r\n";
$headers .= "Content-Type: text/plain; charset=UTF-8\r\n";
$headers .= "From: {$FROM}\r\n";

// 메일 전송
$ok = false;

// 1) mb_send_mail 사용(권장)
if (function_exists('mb_send_mail')) {
    $encodedSubject = mb_encode_mimeheader($subject, 'UTF-8');
    $ok = mb_send_mail($TO, $encodedSubject, $body, $headers);
} else {
    // 2) 일반 mail()로 폴백
    // 일부 환경에서 제목 인코딩 필요할 수 있음
    $ok = mail($TO, $subject, $body, $headers);
}

// 결과 출력
if ($ok) {
    echo json_encode(['ok' => true]);
} else {
    echo json_encode(['ok' => false, 'error' => 'MAIL_SEND_FAILED']);
}
