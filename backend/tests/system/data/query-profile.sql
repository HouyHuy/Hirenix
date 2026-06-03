EXPLAIN SELECT id, title, company_id, city_id, industry_id, status, deadline FROM jobs WHERE status = 'Active' ORDER BY created_at DESC LIMIT 20;
EXPLAIN SELECT id, job_id, candidate_id, status, applied_at FROM applications WHERE candidate_id = 22 ORDER BY applied_at DESC LIMIT 20;
EXPLAIN SELECT id, job_id, candidate_id, status, applied_at FROM applications WHERE job_id = 1 ORDER BY applied_at DESC LIMIT 20;
EXPLAIN SELECT id, conversation_id, sender_id, is_read, created_at FROM messages WHERE conversation_id = 1 ORDER BY created_at DESC LIMIT 50;
EXPLAIN SELECT id, user1_id, user2_id, updated_at FROM conversations WHERE user1_id = 22 OR user2_id = 22 ORDER BY updated_at DESC LIMIT 20;
