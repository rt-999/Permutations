# Permutations

מערכת Client/Server שעוברת על כל התמורות (Permutations) של המספרים 1 עד n, כאשר n בין 1 ל-20.
השרת ב-**.NET 8** והלקוח ב-**Angular 20**. כל החישוב נעשה בשרת, ואף פעם לא מחושבות כל התמורות מראש.

## הרצה

דרישות: .NET 8 SDK, ו-Node.js 20.19 ומעלה (או 22.12 ומעלה).

```bash
# שרת  ->  http://localhost:5080  (Swagger: /swagger)
cd server
dotnet run --project src/Permutations.Api

# לקוח  ->  http://localhost:4200  (חלון טרמינל נפרד)
cd client
npm install
npm start

# בדיקות השרת
cd server
dotnet test
```

בפיתוח הלקוח מעביר את `/api` לשרת דרך `client/proxy.conf.json`, ולכן אין צורך בהגדרות CORS.

## הממשק (API)

| Endpoint | תיאור |
|---|---|
| `POST /api/permutations/start` `{ "n": 5 }` | מאפס את המצב, מחזיר `sessionId` ואת `total` (n!) |
| `POST /api/permutations/next` | מחזיר את התמורה הבאה ואת מספרה הסידורי |
| `GET /api/permutations/all?page=&pageSize=` | מחזיר דף של תמורות. הקריאה הראשונה נכנסת למצב דפדוף |
| `POST /api/permutations/all/exit` | "חזור": יוצא מהדפדוף וממשיך מהתמורה האחרונה שהוצגה |

כל בקשה (חוץ מ-`start`) שולחת את ה-header `X-Session-Id`.
שגיאות חוזרות בפורמט `ProblemDetails` עם שדה `code` יציב (`invalid_n`, `invalid_page`, `session_not_found`, `permutations_exhausted`).

## ארכיטקטורה

### שרת: שכבות

```
Api  ──►  Application  ──►  Domain
              ▲
Infrastructure┘   (מממש את הממשקים של Application)
```

| שכבה | אחריות |
|---|---|
| `Permutations.Domain` | האלגוריתמים והחוקים, בלי תלויות: `FactorialCalculator`, `FactorialUnranker`, `LexicographicNextPermutation`, `SessionState`, החריגות |
| `Permutations.Application` | התהליכים: `PermutationService`, `PageCalculator` (חישובי דפים טהורים), `ISessionStore` (הפורט), `PermutationLimits` |
| `Permutations.Infrastructure` | `InMemorySessionStore` עם נעילה לכל Session, TTL ושירות ניקוי ברקע |
| `Permutations.Api` | Controller, DTOs, המרת `long` ל-string, Middleware שהופך חריגות ל-`ProblemDetails`, ו-Composition Root |

### לקוח

```
core/        תשתית: constants, util/bigint, api (HTTP והמרת string<->bigint), session, interceptors
state/       PermutationStore: המצב של האפליקציה ב-Signals
features/    start, explorer, browse   (Smart components)
shared/      big-paginator, permutation-chips, pipes   (Presentational components)
```

## החלטות והנחות

1. **סדר:** לקסיקוגרפי.
2. **גישה היברידית לאלגוריתמים.** `NextPermutation` הוא O(n) ולכן מתאים ל"הבא" ולמעבר בתוך דף, אבל אי אפשר לקפוץ איתו. `Unrank` (Factorial Number System, O(n²)) מחשב תמורה ישירות מאינדקס ולכן משמש לקפיצה לדף. כל דף: `Unrank` לתמורה הראשונה ואז `NextPermutation` לשאר.
3. **מודל State:** Stateful בזיכרון השרת מאחורי `ISessionStore`. נשמר ב-State רק מה שנחוץ: `N`, `Total`, `LastIndex`, `Current`, `Mode`, `BrowseAnchor`.
4. **מספור דפים יחסי לנקודת הכניסה (anchor).** אם הוצגה תמורה 1000, הדף הראשון מתחיל ב-1001, כמו בדוגמה. הדף האחרון עשוי להיות חלקי. תמורות שלפני ה-anchor אינן נגישות בתצוגת הדפדוף, וחוזרים למסך הבודד דרך "חזור".
5. **"התמורה האחרונה שהוצגה"** = האחרונה בדף האחרון שנצפה.
6. **`StartAPI` לא מציג תמורה.** הוא מחזיר רק את הסך הכול (n!). התמורה הראשונה מוצגת בלחיצה על "הבא".
7. **מספרים גדולים:** `long` בשרת (20! נכנס ב-`long`), `string` ב-JSON, `bigint` בלקוח. ההמרה נעשית במקום אחד בכל צד (`DtoMapper` בשרת, `PermutationApiService` בלקוח), והלקוח משתמש ברכיב דפדוף משלו כי `MatPaginator` לא מתמודד עם מיליארדי דפים.
8. **ולידציה בשני הצדדים.** הלקוח בודק לנוחות המשתמשת. השרת בודק לביטחון, ומגביל: n בין 1 ל-20 ו-`pageSize` עד 100 (ניתן להגדרה ב-`appsettings.json`).
9. **הפרדה בין מגבלה טכנית למדיניות:** `FactorialCalculator.MaxSupported` (גבול `long`) שונה מ-`PermutationLimits.MaxN` (מה שמותר למשתמשת), כי כל אחד משתנה מסיבה אחרת.

## מקביליות

- לכל משתמש `sessionId` אקראי (128 ביט), ו-State נפרד, ולכן משתמשים לא משפיעים זה על זה.
- `ConcurrentDictionary` שומר על המילון, ו-`SemaphoreSlim` לכל Session מונע Race Condition בלחיצות כפולות על אותו Session.
- כל פעולה בודקת ומחשבת הכול לפני שהיא כותבת ל-State, כך שכשל באמצע לא משאיר מצב מקולקל.
- ה-Store הוא Singleton (המצב חייב לשרוד בין בקשות), וה-Service חסר מצב.

## הרחבה ל-Redis

1. מימוש חדש של `ISessionStore` (`RedisSessionStore`) עם serialization של ה-State, מפתח לפי `sessionId` ו-TTL, והחלפה בשורת DI אחת.
2. החלפת הנעילה המקומית באטומיות מבוזרת (Lua script או `WATCH/MULTI`) בתוך `UseAsync`, כי כמה instances יכולים לגשת לאותו מפתח.

הלוגיקה העסקית לא משתנה.

## מגבלות ידועות

- ה-State נשמר בזיכרון, ולכן הוא אובד באתחול השרת, ובסביבה עם כמה instances נדרשים Sticky Sessions (או Redis).
- רענון הדף בלקוח מחזיר למסך הפתיחה. ה-Session הישן נמחק בשרת רק לפי ה-TTL.
- אין אימות משתמשים. ה-`sessionId` הוא הסוד היחיד של ה-Session.

## בדיקות

xUnit לשלוש שכבות: אלגוריתמים (כולל השוואה בין `Unrank` ל-`NextPermutation` על כל האינדקסים של n קטן), `PageCalculator`, ה-Store (כולל 1000 בקשות מקבילות על Session אחד), ה-Service, ובדיקות אינטגרציה של ה-API עם `WebApplicationFactory`.

## רעיונות להמשך

- `RedisSessionStore` ובדיקות אינטגרציה מולו.
- Rate Limiting ולוגים מובנים.
- בדיקות ללקוח (רכיבים וה-Store).
- אפשרות לדפדף גם לפני נקודת הכניסה.
