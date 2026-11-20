CREATE TABLE IF NOT EXISTS departments (
 id INT AUTO_INCREMENT PRIMARY KEY,
 name VARCHAR(100) NOT NULL UNIQUE
) ENGINE=InnoDB;
CREATE TABLE IF NOT EXISTS users (
 id INT AUTO_INCREMENT PRIMARY KEY,
 full_name VARCHAR(120) NOT NULL,
 email VARCHAR(180) NOT NULL UNIQUE,
 password_hash VARCHAR(400) NOT NULL,
 role VARCHAR(30) NOT NULL,
 department_id INT NOT NULL,
 active BOOLEAN NOT NULL DEFAULT TRUE,
 FOREIGN KEY (department_id) REFERENCES departments(id)
) ENGINE=InnoDB;
CREATE TABLE IF NOT EXISTS forms (
 id INT AUTO_INCREMENT PRIMARY KEY,
 title VARCHAR(150) NOT NULL,
 description TEXT NOT NULL,
 department_id INT NOT NULL,
 created_by INT NOT NULL,
 status VARCHAR(20) NOT NULL DEFAULT 'Draft',
 created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 FOREIGN KEY (department_id) REFERENCES departments(id),
 FOREIGN KEY (created_by) REFERENCES users(id)
) ENGINE=InnoDB;
CREATE TABLE IF NOT EXISTS questions (
 id INT AUTO_INCREMENT PRIMARY KEY,
 form_id INT NOT NULL,
 label VARCHAR(200) NOT NULL,
 field_type VARCHAR(30) NOT NULL,
 options_text TEXT NOT NULL,
 required BOOLEAN NOT NULL,
 position INT NOT NULL,
 FOREIGN KEY (form_id) REFERENCES forms(id) ON DELETE CASCADE
) ENGINE=InnoDB;
CREATE TABLE IF NOT EXISTS submissions (
 id INT AUTO_INCREMENT PRIMARY KEY,
 reference_no VARCHAR(40) NOT NULL UNIQUE,
 form_id INT NOT NULL,
 submitted_by INT NOT NULL,
 status VARCHAR(40) NOT NULL DEFAULT 'Draft',
 revision INT NOT NULL DEFAULT 1,
 first_reviewer_id INT NULL,
 updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 FOREIGN KEY (form_id) REFERENCES forms(id),
 FOREIGN KEY (submitted_by) REFERENCES users(id),
 FOREIGN KEY (first_reviewer_id) REFERENCES users(id),
 INDEX idx_submission_status(status)
) ENGINE=InnoDB;
CREATE TABLE IF NOT EXISTS answers (
 id INT AUTO_INCREMENT PRIMARY KEY,
 submission_id INT NOT NULL,
 revision INT NOT NULL,
 question_id INT NOT NULL,
 answer_text TEXT NOT NULL,
 UNIQUE KEY uq_answer(submission_id,revision,question_id),
 FOREIGN KEY (submission_id) REFERENCES submissions(id),
 FOREIGN KEY (question_id) REFERENCES questions(id)
) ENGINE=InnoDB;
CREATE TABLE IF NOT EXISTS reviews (
 id INT AUTO_INCREMENT PRIMARY KEY,
 submission_id INT NOT NULL,
 revision INT NOT NULL,
 reviewer_id INT NOT NULL,
 stage VARCHAR(30) NOT NULL,
 decision VARCHAR(30) NOT NULL,
 comment TEXT NOT NULL,
 created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 FOREIGN KEY (submission_id) REFERENCES submissions(id),
 FOREIGN KEY (reviewer_id) REFERENCES users(id)
) ENGINE=InnoDB;
CREATE TABLE IF NOT EXISTS audit_logs (
 id INT AUTO_INCREMENT PRIMARY KEY,
 user_id INT NULL,
 action VARCHAR(100) NOT NULL,
 details VARCHAR(500) NOT NULL,
 created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 FOREIGN KEY (user_id) REFERENCES users(id)
) ENGINE=InnoDB;
