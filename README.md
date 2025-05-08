## Описание Проекта: EventApp API

**EventApp API** - это бэкэнд веб-приложения для управления событиями, разработанный на платформе **.NET 9**. Приложение предоставляет RESTful API для создания, просмотра, обновления и удаления событий, категорий событий, а также для регистрации пользователей на эти события.

**Основные возможности:**

*   **Управление Событиями:** CRUD операции для событий (название, описание, дата, место, макс. участников, изображение, категория).
*   **Управление Категориями:** CRUD операции для категорий событий.
*   **Регистрация Пользователей:** Регистрация/отмена регистрации пользователей на события с проверками (наличие мест, дата события, отсутствие дубликатов).
*   **Получение Списков:** Возможность получения списка событий с фильтрацией (по дате, месту, категории) и пагинацией. Получение списка участников события.
*   **Аутентификация:** Реализована JWT Bearer аутентификация для защиты эндпоинтов.
*   **Загрузка Файлов:** Возможность загрузки изображений для событий (сохраняются локально в `wwwroot/images` с уникальными именами).
*   **База Данных:** Используется PostgreSQL для хранения данных.
*   **Миграции:** Используются миграции Entity Framework Core для управления схемой базы данных (с возможностью автоматического применения при старте для разработки).

**Технологический стек:**

*   **Платформа:** .NET 9 / ASP.NET Core
*   **ORM:** Entity Framework Core 9
*   **База Данных:** PostgreSQL (с использованием драйвера `Npgsql.EntityFrameworkCore.PostgreSQL`)
*   **API и Сервисы:** RESTful API, JWT Bearer аутентификация (`Microsoft.AspNetCore.Authentication.JwtBearer`), AutoMapper, FluentValidation
*   **API Документация:** Scalar (OpenAPI/Swagger через `Microsoft.AspNetCore.OpenApi` и `Scalar.AspNetCore`)
*   **Логирование:** Serilog (с выводом в консоль и файл)
*   **Обработка файлов:** Локальное хранилище (`wwwroot`)
*   **Контейнеризация:** Docker, Docker Compose
*   **Тестирование:** xUnit, Moq, AutoFixture (с AutoMoq), FluentAssertions, EF Core InMemory Provider

**Архитектура (опционально):**

*   Использован Repository Pattern для доступа к данным, Service Layer для бизнес-логики, Controllers для обработки API запросов.

---

## Инструкции по Запуску Проекта (с использованием Docker Compose)

Данные инструкции предполагают, что проект будет запущен локально с использованием Docker и Docker Compose.

**1. Предварительные требования:**

*   **Docker:** Установленный и запущенный Docker Desktop (Windows, macOS) или Docker Engine/Docker Compose (Linux). [Официальный сайт Docker](https://www.docker.com/products/docker-desktop/)
*   **.NET SDK:** Установленный .NET SDK (версии 9 или основной версии проекта), необходим для работы с сертификатами (`dotnet dev-certs`). [Официальный сайт .NET](https://dotnet.microsoft.com/download)
*   **Git:** Для клонирования репозитория.

**2. Настройка и Запуск:**

1.  **Клонировать репозиторий:**
    ```bash
    git clone https://github.com/For2ate/Events-Web-application.git
    cd Events-Web-application
    ```

2.  **Настройка HTTPS Сертификата Разработки:**
    Docker-контейнер будет использовать HTTPS. Для локальной разработки необходимо создать, экспортировать и доверить сертификат:

    *   **Экспортировать сертификат в PFX формат** (для Kestrel внутри контейнера):
        ```powershell
        # В PowerShell (Windows) - убедитесь, что папка .aspnet\https существует
        dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\EventApi.pfx -p pass
        ```
        ```bash
        # В Bash (Linux/macOS) - убедитесь, что папка ~/.aspnet/https существует
        dotnet dev-certs https -ep ${HOME}/.aspnet/https/EventApi.pfx -p pass
        ```
        *   `-ep`: Путь для экспорта. Файл `EventApi.pfx` будет создан в стандартной папке ASP.NET Core для сертификатов в вашем домашнем каталоге.
        *   `-p pass`: Устанавливает пароль `pass` для PFX файла (этот пароль используется в `docker-compose.yml`). **Имя файла (`EventApi.pfx`) и пароль (`pass`) должны точно совпадать** с теми, что указаны в переменных окружения `ASPNETCORE_Kestrel__Certificates__Default__Path` и `ASPNETCORE_Kestrel__Certificates__Default__Password` в `docker-compose.yml`.

    *   **Доверить сертификату на хост-машине** (чтобы браузер не выдавал предупреждений):
        ```powershell
        # В PowerShell или Командной строке Windows/Linux/macOS
        dotnet dev-certs https --trust
        ```
        (Может потребоваться подтверждение установки сертификата в системное хранилище доверенных сертификатов).

3.  **Создать и настроить файл `.env`:**
    *   В корневой папке проекта (рядом с `docker-compose.yml`) создайте файл `.env`.
    *   Скопируйте в него следующее содержимое, **обязательно заменив `YourSuperSecretKeyHere...` на ваш реальный надежный JWT секретный ключ**:

        ```dotenv
        # .env файл

        # Секреты для базы данных (можно оставить как есть, если пароль совпадает с docker-compose)
        POSTGRES_USER=postgres
        POSTGRES_PASSWORD=mysecretpassword # Убедитесь, что совпадает с паролем в docker-compose для сервиса db
        POSTGRES_DB=Event.Web.App
        DB_HOST=db
        DB_PORT=5432
        DB_CONNECTION_STRING=Host=${DB_HOST};Port=${DB_PORT};Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}

        # ВАЖНО: Установите ваш реальный JWT секретный ключ
        JWT_SECRET_KEY=YourSuperSecretKeyHereWhichIsVeryLongAndSecure123!@#

        # Пароль от PFX сертификата (должен совпадать с командой экспорта и docker-compose)
        CERT_PASSWORD=pass
        ```

4.  **Собрать и запустить контейнеры:**
    Откройте терминал в корневой папке проекта (где лежит `docker-compose.yml`) и выполните команду:
    ```bash
    docker-compose up --build -d
    ```
    *   `up`: Запускает сервисы, описанные в `docker-compose.yml`.
    *   `--build`: Принудительно пересобирает образы перед запуском (важно после изменений в Dockerfile или коде).
    *   `-d`: Запускает контейнеры в фоновом режиме (detached mode).

**3. Доступ к Приложению:**

*   **API HTTP:** `http://localhost:5274` (порт 5274 на хосте сопоставлен с портом 8080 в контейнере)
*   **API HTTPS:** `https://localhost:7026` (порт 7026 на хосте сопоставлен с портом 8081 в контейнере)
*   **Документация API (Scalar UI):** Откройте `https://localhost:7026/scalar` в вашем браузере (рекомендуется использовать HTTPS). Вы также можете использовать `http://localhost:5274/scalar`.
*   **База Данных (при необходимости):** PostgreSQL доступен на хосте по адресу `localhost:54399` (порт 54399 сопоставлен с портом 5432 в контейнере). Используйте пользователя `postgres` и пароль из вашего `.env` файла.

**4. Остановка Приложения:**

Чтобы остановить и удалить контейнеры, выполните в терминале в той же папке:

```bash
docker-compose down
```

*   Эта команда остановит и удалит контейнеры, но **не удалит** volume с данными PostgreSQL (`postgres_data`).
*   Если вы хотите удалить и volume с данными БД (например, для полного сброса), используйте:
    ```bash
    docker-compose down -v
    ```

---

**Дополнительные Заметки:**

*   Убедитесь, что порты, указанные в `docker-compose.yml` (например, `5274`, `7026`, `54399`), не заняты другими приложениями на вашем компьютере.
*   Проверьте логи контейнеров, если возникнут проблемы при запуске: `docker-compose logs api` или `docker-compose logs db`.