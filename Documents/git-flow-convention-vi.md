# Unity Project Git Flow Convention - Complete Guide

## 🚀 Mục tiêu
- Quản lý dự án Unity theo Git Flow chuẩn với tích hợp Jira
- Tránh xung đột scene và file `.meta`
- Giữ code và asset gọn gàng, dễ quản lý
- Đảm bảo quy trình phát triển ổn định và có thể mở rộng
- Theo dõi tiến độ phát triển thông qua Jira tickets
- Hướng dẫn thực hành từ lý thuyết đến thực tế

---

## 📂 Cấu trúc Branch

### 🌿 Main Branches
- **`master`** → Code ổn định, đã test, sẵn sàng build và deploy
- **`develop`** → Code phát triển, tích hợp các tính năng từ feature branches

### 🌱 Supporting Branches
- **`feature/*`** → Nhánh cho từng tính năng mới
- **`release/*`** → Chuẩn bị phát hành, chỉ fix bug nhỏ và polish
- **`hotfix/*`** → Sửa lỗi khẩn cấp từ `master`
- **`support/*`** → Hỗ trợ các phiên bản cũ

---

## 🛠 Thiết lập Git Flow

```bash
# Đã được thiết lập với lệnh:
git flow init -d

# Cấu hình mặc định:
# - Production branch: master
# - Development branch: develop
# - Feature prefix: feature/
# - Release prefix: release/
# - Hotfix prefix: hotfix/
# - Support prefix: support/
```

---

## 🔗 Tích hợp Jira Workflow

### 📋 Quản lý Jira Ticket
- **Epic:** Tính năng lớn được chia thành nhiều story
- **Story:** User story riêng lẻ (PROJ-123, PROJ-124, etc.)
- **Bug:** Báo cáo lỗi (PROJ-456, PROJ-457, etc.)
- **Task:** Công việc phát triển (PROJ-789, PROJ-790, etc.)

### 🔄 Quy trình Trạng thái Jira
1. **To Do** → Ticket được tạo, sẵn sàng phát triển
2. **In Progress** → Developer bắt đầu làm việc (tạo feature branch)
3. **In Review** → Giai đoạn review code (tạo pull request)
4. **Done** → Tính năng hoàn thành, merge vào develop
5. **Closed** → Đã release lên production

### 📝 Best Practices cho Jira Ticket
- Luôn bao gồm acceptance criteria trong mô tả ticket
- Thêm screenshot/mockup cho thay đổi UI
- Liên kết các ticket liên quan (blocks, relates to)
- Cập nhật time tracking trong Jira
- Thêm commit hash vào comment ticket để theo dõi

---

## 🏷️ Git Tag Guidelines

### 📅 Khi nào sử dụng Tag

**1. 🚀 Release Tags (Bắt buộc)**
```bash
# Khi hoàn thành release
git flow release finish 1.0.0
# Tự động tạo tag: v1.0.0

# Hoặc tạo tag thủ công
git tag -a v1.0.0 -m "Release version 1.0.0 - User Authentication System"
git push origin v1.0.0
```

**2. 🔥 Hotfix Tags (Bắt buộc)**
```bash
# Khi hoàn thành hotfix
git flow hotfix finish PROJ-999-critical-fix
# Tự động tạo tag: v1.0.1

# Hoặc tạo tag thủ công
git tag -a v1.0.1 -m "Hotfix v1.0.1 - Fix critical game crash"
git push origin v1.0.1
```

**3. 🎯 Milestone Tags (Tùy chọn)**
```bash
# Đánh dấu các milestone quan trọng
git tag -a milestone-alpha -m "Alpha version completed"
git tag -a milestone-beta -m "Beta version completed"
git push origin milestone-alpha
```

### 🏷️ Tag Naming Convention

**Release Tags:**
- `v1.0.0` - Major release
- `v1.1.0` - Minor release  
- `v1.0.1` - Patch/hotfix
- `v2.0.0` - Major version update

**Hotfix Tags:**
- `v1.0.1` - Hotfix cho v1.0.0
- `v1.0.2` - Hotfix tiếp theo
- `v1.1.1` - Hotfix cho v1.1.0

**Special Tags:**
- `milestone-alpha` - Alpha milestone
- `milestone-beta` - Beta milestone
- `demo-v1` - Demo version

### 📋 Tag Workflow với Jira

**1. Tạo Release Tag:**
```bash
# Trong Jira: Tạo release version "v1.0.0"
# Trong Git: 
git flow release finish 1.0.0
git push origin v1.0.0

# Cập nhật Jira release với git tag link
```

**2. Tạo Hotfix Tag:**
```bash
# Trong Jira: Tạo urgent bug ticket PROJ-999
# Trong Git:
git flow hotfix finish PROJ-999-critical-fix
git push origin v1.0.1

# Cập nhật Jira ticket với hotfix tag
```

**3. Liên kết Tag với Jira:**
- Thêm git tag link vào Jira release
- Comment commit hash trong Jira ticket
- Cập nhật release notes với tag information

---

## 📋 Quy trình làm việc chi tiết

### Bước 1: 🆕 Bắt đầu tính năng mới

#### 1.1 Tạo Feature Branch
```bash
# Tạo feature branch từ develop sử dụng Jira ticket ID
git flow feature start PROJ-123-feature-name

# Hoặc với mô tả chi tiết
git flow feature start PROJ-123-feature-name -d "JIRA-123: Thêm hệ thống đăng nhập"

# Ví dụ thực tế:
git flow feature start PROJ-123-player-movement
```

**Quy tắc đặt tên feature với Jira:**
- `feature/PROJ-123-user-authentication`
- `feature/PROJ-456-inventory-system`
- `feature/PROJ-789-level-1-design`
- `feature/PROJ-101-audio-integration`

**Định dạng Jira Ticket:**
- Sử dụng project key (e.g., PROJ, GAME, UNITY) + số ticket
- Bao gồm tên mô tả ngắn sau số ticket
- Ví dụ: `PROJ-123-user-auth` cho ticket PROJ-123

#### 1.2 Kiểm tra trạng thái
```bash
# Kiểm tra branch hiện tại
git branch
# Output: * feature/PROJ-123-player-movement

# Kiểm tra status
git status
# Output: On branch feature/PROJ-123-player-movement
#         Your branch is up to date with 'origin/develop'
```

#### 1.3 Cập nhật Jira
- Chuyển ticket `PROJ-123` từ "To Do" → "In Progress"
- Thêm comment: "Started development on feature branch"

---

### Bước 2: 🔄 Phát triển tính năng

#### 2.1 Làm việc trên tính năng
```bash
# Tạo và chỉnh sửa files
# Ví dụ: Assets/Scripts/PlayerMovement.cs

# Commit thường xuyên với message rõ ràng bao gồm Jira ticket
git add Assets/Scripts/PlayerMovement.cs
git commit -m "feat(PROJ-123): add basic player movement script"

# Thêm input handling
git add Assets/Scripts/InputManager.cs
git commit -m "feat(PROJ-123): implement WASD input handling"

# Thêm animation
git add Assets/Animations/PlayerWalk.anim
git commit -m "feat(PROJ-123): add player walk animation"
```

**Commit Message Convention với Jira:**
- `feat(PROJ-123):` - Tính năng mới
- `fix(PROJ-456):` - Sửa lỗi
- `docs(PROJ-789):` - Cập nhật tài liệu
- `style(PROJ-101):` - Thay đổi format code
- `refactor(PROJ-202):` - Tái cấu trúc code
- `test(PROJ-303):` - Thêm test
- `chore(PROJ-404):` - Công việc bảo trì

**Tích hợp Jira:**
- Luôn bao gồm số Jira ticket trong commit message
- Sử dụng định dạng: `type(PROJ-123): mô tả`
- Liên kết commit với Jira ticket để theo dõi tự động

#### 2.2 Push lên remote
```bash
# Push feature branch lên remote
git push origin feature/PROJ-123-player-movement

# Kết quả: Branch được push lên GitHub/GitLab
```

#### 2.3 Sync với develop (nếu cần)
```bash
# Nếu develop có update mới từ team khác
git checkout develop
git pull origin develop
git checkout feature/PROJ-123-player-movement
git merge develop

# Resolve conflicts nếu có
# Sau đó push lại
git push origin feature/PROJ-123-player-movement
```

#### 2.4 Test tính năng
```bash
# Test trong Unity Editor
# - Kiểm tra player movement hoạt động
# - Đảm bảo không có lỗi compile
# - Test scene không bị broken
```

---

### Bước 3: ✅ Hoàn thành tính năng

#### 3.1 Kiểm tra cuối cùng
```bash
# Kiểm tra status
git status
# Đảm bảo tất cả changes đã được commit

# Kiểm tra log
git log --oneline -5
# Xem 5 commit gần nhất
```

#### 3.2 Finish feature
```bash
# Merge feature vào develop
git flow feature finish PROJ-123-player-movement

# Kết quả:
# - Feature được merge vào develop
# - Feature branch bị xóa
# - Bạn được chuyển về develop branch
# - Git hiển thị summary của merge
```

#### 3.3 Push develop
```bash
# Push develop lên remote
git push origin develop

# Kết quả: Develop branch được cập nhật trên remote
```

#### 3.4 Cập nhật Jira
- Chuyển ticket `PROJ-123` từ "In Progress" → "Done"
- Thêm comment với commit hash
- Link feature branch vào ticket
- Update time tracking

---

### Bước 4: 🚀 Chuẩn bị phát hành

#### 4.1 Quyết định tạo Release
```bash
# Chỉ tạo release khi:
# - Có đủ features để release
# - Code đã được test
# - Team đồng ý release

# Tạo release branch từ develop
git flow release start 1.0.0

# Kết quả:
# - Tạo branch: release/1.0.0
# - Tự động checkout sang release branch
# - Branch được tạo từ develop
```

#### 4.2 Làm việc trên Release
```bash
# CHỈ sửa bug nhỏ và polish, KHÔNG thêm tính năng mới

# Ví dụ: Điều chỉnh movement speed
git add Assets/Scripts/PlayerMovement.cs
git commit -m "fix(PROJ-999): adjust player movement speed for better gameplay"

# Update version number
git add ProjectSettings/ProjectVersion.txt
git commit -m "chore(PROJ-999): update version to 1.0.0"

# Fix UI scaling
git add Assets/UI/MainMenu.prefab
git commit -m "fix(PROJ-999): fix UI scaling issues for release"
```

#### 4.3 Test Release
```bash
# Test toàn bộ tính năng trong release branch
# - Build game
# - Test trên target platform
# - Kiểm tra performance
# - Verify bug fixes
```

#### 4.4 Cập nhật Jira Release
- Tạo Jira release version "v1.0.0"
- Chuyển các ticket đã hoàn thành vào release
- Cập nhật release notes
- Set release date

---

### Bước 5: 🏷️ Hoàn thành Release

#### 5.1 Finish Release
```bash
# Hoàn thành release
git flow release finish 1.0.0

# Git sẽ mở editor để viết tag message
# Bạn cần viết message mô tả release
```

#### 5.2 Viết Tag Message
**Trong editor, viết message như sau:**

```
Release v1.0.0 - Player Movement System

Features included:
- Implemented WASD player movement controls
- Added player walk animation system
- Integrated input management system
- Created responsive UI controls

Bug fixes:
- Adjusted player movement speed for optimal gameplay
- Fixed UI scaling issues across different resolutions
- Resolved animation transition problems

Technical improvements:
- Optimized movement performance
- Enhanced collision detection
- Improved code structure

Build: Unity 2022.3 LTS
Target platforms: Windows, macOS
Release date: [Ngày release]
```

#### 5.3 Sau khi lưu tag message
```bash
# Git sẽ tự động thực hiện:
# 1. Tạo annotated tag: v1.0.0
# 2. Merge release vào master
# 3. Merge release vào develop
# 4. Xóa release branch
# 5. Chuyển bạn về develop branch

# Hiển thị summary:
# Summary of actions:
# - Release branch 'release/1.0.0' has been merged into 'master'
# - Tag 'v1.0.0' has been back-merged into 'develop'
# - Release branch 'release/1.0.0' has been locally deleted
# - You are now on branch 'develop'
```

#### 5.4 Push tất cả lên remote
```bash
# Push develop
git push origin develop

# Push master
git push origin master

# Push tag
git push origin v1.0.0

# Hoặc push tất cả tags
git push origin --tags
```

#### 5.5 Cập nhật Jira cuối cùng
- Chuyển ticket `PROJ-123` từ "Done" → "Closed"
- Update release status trong Jira
- Add git tag link vào Jira release
- Update release notes với final information

---

### Bước 6: 🔥 Sửa lỗi khẩn cấp (Nếu cần)

#### 6.1 Tạo Hotfix
```bash
# Tạo hotfix từ master
git flow hotfix start PROJ-999-critical-bug-fix

# Kết quả: Tạo branch hotfix/PROJ-999-critical-bug-fix từ master
```

#### 6.2 Sửa lỗi khẩn cấp
```bash
# Sửa lỗi
git add Assets/Scripts/PlayerMovement.cs
git commit -m "fix(PROJ-999): sửa lỗi crash game khẩn cấp"

# Test hotfix
# Build và test trên target platform
```

#### 6.3 Hoàn thành Hotfix
```bash
# Hoàn thành hotfix
git flow hotfix finish PROJ-999-critical-bug-fix

# Viết tag message:
```

**Hotfix Tag Message:**
```
Hotfix v1.0.1 - Fix critical player crash issue

Bug fixes:
- Fixed player crash when moving at high speed
- Improved movement collision detection
- Enhanced error handling for edge cases

Critical fix for production stability.
Build: Unity 2022.3 LTS
Target: Windows, macOS
```

**Quy trình Hotfix trong Jira:**
- Tạo ticket bug khẩn cấp trong Jira (Priority: Critical/Blocker)
- Sử dụng prefix ticket hotfix đặc biệt (e.g., PROJ-999)
- Cập nhật trạng thái ticket ngay khi hotfix được deploy
- Liên kết commit hotfix với ticket khẩn cấp

---

## 🎯 Kết quả cuối cùng

### 🌿 Branch Structure sau Release:
```
master (stable) ← v1.0.0 tag
    ↑
develop (development)
    ↑
feature branches (new features)
```

### 🏷️ Tags được tạo:
- `v1.0.0` - Release với player movement system
- `v1.0.1` - Hotfix cho critical bug (nếu có)

### 📊 Jira Status:
- `PROJ-123` - Closed (completed and released)
- Release version "v1.0.0" - Released

### 📁 Files được quản lý:
- `Assets/Scripts/PlayerMovement.cs` - Core movement logic
- `Assets/Scripts/InputManager.cs` - Input handling
- `Assets/Animations/PlayerWalk.anim` - Player animation
- `ProjectSettings/ProjectVersion.txt` - Version info

---

## 🎯 Unity-Specific Guidelines

### 📁 File Management
- **Scene files:** Chỉ commit scene đã hoàn thiện, tránh commit scene đang edit
- **Meta files:** LUÔN commit cùng với asset tương ứng
- **Build settings:** Commit `ProjectSettings.asset` khi thay đổi build config
- **Package manifest:** Commit `Packages/manifest.json` khi thêm/remove package

### 🚫 Files to Ignore
```gitignore
# Unity generated files
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/

# MemoryCaptures can get excessive in size
[Mm]emoryCaptures/

# Asset meta data should only be ignored when the corresponding asset is also ignored
!/[Aa]ssets/**/*.meta

# Visual Studio cache directory
.vs/

# Gradle cache directory
.gradle/

# Autogenerated VS/MD/Consulo solution and project files
ExportedObj/
.consulo/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db

# Unity3D generated meta files
*.pidb.meta
*.pdb.meta
*.mdb.meta

# Unity3D generated file on crash reports
sysinfo.txt

# Builds
*.apk
*.aab
*.unitypackage
*.app

# Crashlytics generated file
crashlytics-build.properties

# OS generated
.DS_Store
.DS_Store?
._*
.Spotlight-V100
.Trashes
ehthumbs.db
Thumbs.db
```

### 🔄 Scene Management
- **Main Scene:** Chỉ edit khi cần thiết
- **Test Scene:** Sử dụng cho testing tính năng
- **Build Scene:** Scene cuối cùng cho build

---

## 🚨 Lưu ý quan trọng

### ✅ Nên làm:
- Test kỹ trước khi finish feature
- Commit thường xuyên với message rõ ràng
- Chỉ fix bug nhỏ trong release branch
- Viết tag message chi tiết và mô tả đầy đủ
- Cập nhật Jira sau mỗi bước
- Sync với develop thường xuyên
- Test tính năng trước khi merge
- Tạo Pull Request cho mọi feature

### ❌ Không nên làm:
- Thêm tính năng mới trong release branch
- Commit code chưa test
- Bỏ qua việc cập nhật Jira
- Viết tag message quá ngắn gọn
- Push code chưa hoàn thiện
- Merge code có lỗi compile
- Bỏ qua việc test scene

### 🔧 Troubleshooting:
- **Conflict khi merge:** Resolve conflicts trong Unity Editor
- **Tag message editor:** Sử dụng vim/nano/VS Code để edit
- **Push failed:** Kiểm tra network và permissions
- **Jira sync issues:** Manual update nếu cần
- **Scene conflicts:** Backup scene trước khi merge
- **Meta file issues:** Regenerate meta files trong Unity

---

## 📚 Commands Reference

### 🔍 Kiểm tra trạng thái:
```bash
git branch          # Xem branch hiện tại
git status          # Xem trạng thái files
git log --oneline   # Xem commit history
git tag -l          # Xem tất cả tags
```

### 🔄 Branch operations:
```bash
git flow feature start <name>     # Tạo feature
git flow feature finish <name>    # Hoàn thành feature
git flow release start <version>  # Tạo release
git flow release finish <version> # Hoàn thành release
git flow hotfix start <name>      # Tạo hotfix
git flow hotfix finish <name>     # Hoàn thành hotfix
```

### 📤 Push operations:
```bash
git push origin <branch>          # Push branch
git push origin <tag>             # Push tag
git push origin --tags            # Push tất cả tags
```

### 🏷️ Tag operations:
```bash
git tag -a v1.0.0 -m "Release message"  # Tạo annotated tag
git show v1.0.0                        # Xem chi tiết tag
git tag -d v1.0.0                      # Delete tag local
git push origin --delete v1.0.0        # Delete tag remote
```

### 🧹 Cleanup:
```bash
git branch --merged | grep -v "\*" | xargs -n 1 git branch -d  # Clean merged branches
git remote prune origin                                           # Clean remote tracking
```

---

## 📞 Support

Nếu gặp vấn đề với Git Flow:
1. Kiểm tra tài liệu này trước
2. Hỏi team lead hoặc senior developer
3. Tạo issue trong project repository
4. Tham khảo troubleshooting section

---

## 📅 Version History

- **v1.0.0** - Initial Git Flow setup
- **v1.1.0** - Added Unity-specific guidelines
- **v1.2.0** - Enhanced best practices and troubleshooting
- **v1.3.0** - Added Jira workflow integration
- **v1.4.0** - Added comprehensive Git Tag guidelines
- **v2.0.0** - Complete guide with step-by-step instructions

---

*Tài liệu này kết hợp lý thuyết Git Flow với hướng dẫn thực hành chi tiết, dựa trên kinh nghiệm thực tế và best practices cho Unity projects.*
