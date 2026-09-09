-- Gives every crest-less team a generated SVG shield: a solid colour derived
-- from a hash of the name (so each team gets its own hue) with the team's
-- initials in white. Safe to re-run: only rows where "Crest" IS NULL are touched.
--
--   docker exec -i fairplay-pg psql -U postgres -d FairPlaySports -v ON_ERROR_STOP=1 < scripts/backfill-team-crests.sql

SET client_encoding TO 'UTF8';

BEGIN;

WITH d AS (
    SELECT
        "Id",
        (('x' || substr(md5("Name"), 1, 6))::bit(24)::int % 360) AS hue,
        upper(
            left(split_part(btrim("Name"), ' ', 1), 1)
            || left(split_part(btrim("Name"), ' ', 2), 1)
        ) AS initials
    FROM "Teams"
    WHERE "Crest" IS NULL
)
UPDATE "Teams" AS t
SET
    "CrestContentType" = 'image/svg+xml',
    "Crest" = convert_to(
        '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" width="100" height="100">'
        || '<path d="M50 6 L88 19 V49 C88 73 72 89 50 95 C28 89 12 73 12 49 V19 Z" '
        || 'fill="hsl(' || d.hue || ',58%,46%)" stroke="hsl(' || d.hue || ',58%,32%)" '
        || 'stroke-width="4" stroke-linejoin="round"/>'
        || '<text x="50" y="55" text-anchor="middle" dominant-baseline="middle" '
        || 'font-family="Arial, Helvetica, sans-serif" font-size="30" font-weight="700" '
        || 'fill="#ffffff">' || d.initials || '</text>'
        || '</svg>',
        'UTF8')
FROM d
WHERE t."Id" = d."Id";

COMMIT;

SELECT
    count(*) FILTER (WHERE "Crest" IS NOT NULL) AS with_crest,
    count(*) FILTER (WHERE "Crest" IS NULL) AS without_crest
FROM "Teams";
