-- Seeds 400 example teams into the "Teams" table (Andalusian flavour, to match
-- the rows already there). Safe to re-run: ON CONFLICT ("Name") DO NOTHING skips
-- any name that already exists. All teams are created without a crest.
--
--   docker exec -i fairplay-pg psql -U postgres -d FairPlaySports -v ON_ERROR_STOP=1 < scripts/seed-teams.sql

SET client_encoding TO 'UTF8';

BEGIN;

-- Batch 1: 100 teams around the province of Sevilla, one per town/district.
INSERT INTO "Teams"
    ("Id", "Name", "Coach", "City", "Type", "Division", "Category", "CreatedAt", "Active")
SELECT
    gen_random_uuid(),
    place || ' ' || (ARRAY['CF','CD','FS','Balompié','Atlético','Unión','Promesas','Juvenil','Deportivo','Athletic'])[1 + (i % 10)],
    (ARRAY['Javier','Miguel','Antonio','Luis','Carlos','Pablo','Sergio','David','Iker','Unai','Fran','Diego'])[1 + (i % 12)]
        || ' ' ||
    (ARRAY['García','Fernández','Martínez','López','Sánchez','Romero','Torres','Navarro','Gil','Vidal','Cabrera','Moya'])[1 + ((i * 5) % 12)],
    city,
    (ARRAY['Football11','Football11','Football11','Football8','Futsal','BeachSoccer'])[1 + (i % 6)],
    (ARRAY['HonorDivision','RegionalLeague','First','Second'])[1 + (i % 4)],
    (ARRAY['Chupetes','Prebenjamines','Benjamines','Alevines','Infantiles','Cadetes','Juveniles','Aficionados','Veteranos'])[1 + (i % 9)],
    now() - ((i * 3) || ' days')::interval,
    (i % 4 <> 0)
FROM (
    SELECT
        g.i,
        (ARRAY[
            'Arahal','Benacazón','Aguadulce','Alanís','Albaida del Aljarafe',
            'Alcalá del Río','Alcolea del Río','Algámitas','Almadén de la Plata','Almensilla',
            'Aznalcázar','Aznalcóllar','Badolatosa','Bollullos de la Mitación','Brenes',
            'Burguillos','Las Cabezas de San Juan','La Campana','Cantillana','Cañada Rosal',
            'Casariche','Castilblanco de los Arroyos','Castilleja de Guzmán','Castilleja de la Cuesta','Castilleja del Campo',
            'El Castillo de las Guardas','Cazalla de la Sierra','Constantina','El Coronil','El Cuervo de Sevilla',
            'Espartinas','Fuentes de Andalucía','El Garrobo','Gelves','Gerena',
            'Gilena','Guadalcanal','Guillena','Herrera','Huévar del Aljarafe',
            'Isla Mayor','La Algaba','Lantejuela','Lora de Estepa','Lora del Río',
            'La Luisiana','Los Molares','Montellano','El Madroño','Mairena del Alcor',
            'Marinaleda','Martín de la Jara','Los Palacios y Villafranca','Palomares del Río','Paradas',
            'Pedrera','Peñaflor','El Pedroso','Pilas','La Puebla de Cazalla',
            'La Puebla de los Infantes','La Puebla del Río','Pruna','El Real de la Jara','La Roda de Andalucía',
            'El Ronquillo','El Rubio','Salteras','San Nicolás del Puerto','Sanlúcar la Mayor',
            'Santiponce','El Saucejo','Tocina','Umbrete','Valencina de la Concepción',
            'El Viso del Alcor','Villamanrique de la Condesa','Villanueva del Ariscal','Heliópolis','Bami',
            'Pino Montano','San Pablo','Torreblanca','El Porvenir','La Palmera',
            'Bellavista','San Jerónimo','Valdezorras','Sevilla Este','Rochelambert',
            'Cerro del Águila','Tiro de Línea','La Oliva','El Plantinar','La Buhaira',
            'Santa Clara','Los Pajaritos','El Tardón','Alcosa','Padre Pío'
        ])[g.i] AS place,
        CASE WHEN g.i >= 79 THEN 'Sevilla' ELSE (ARRAY[
            'Arahal','Benacazón','Aguadulce','Alanís','Albaida del Aljarafe',
            'Alcalá del Río','Alcolea del Río','Algámitas','Almadén de la Plata','Almensilla',
            'Aznalcázar','Aznalcóllar','Badolatosa','Bollullos de la Mitación','Brenes',
            'Burguillos','Las Cabezas de San Juan','La Campana','Cantillana','Cañada Rosal',
            'Casariche','Castilblanco de los Arroyos','Castilleja de Guzmán','Castilleja de la Cuesta','Castilleja del Campo',
            'El Castillo de las Guardas','Cazalla de la Sierra','Constantina','El Coronil','El Cuervo de Sevilla',
            'Espartinas','Fuentes de Andalucía','El Garrobo','Gelves','Gerena',
            'Gilena','Guadalcanal','Guillena','Herrera','Huévar del Aljarafe',
            'Isla Mayor','La Algaba','Lantejuela','Lora de Estepa','Lora del Río',
            'La Luisiana','Los Molares','Montellano','El Madroño','Mairena del Alcor',
            'Marinaleda','Martín de la Jara','Los Palacios y Villafranca','Palomares del Río','Paradas',
            'Pedrera','Peñaflor','El Pedroso','Pilas','La Puebla de Cazalla',
            'La Puebla de los Infantes','La Puebla del Río','Pruna','El Real de la Jara','La Roda de Andalucía',
            'El Ronquillo','El Rubio','Salteras','San Nicolás del Puerto','Sanlúcar la Mayor',
            'Santiponce','El Saucejo','Tocina','Umbrete','Valencina de la Concepción',
            'El Viso del Alcor','Villamanrique de la Condesa','Villanueva del Ariscal'
        ])[g.i] END AS city
    FROM generate_series(1, 100) AS g(i)
) AS rows
ON CONFLICT ("Name") DO NOTHING;

-- Batch 2: 300 teams across the other Andalusian provinces, 60 towns x 5 club
-- names ("<town> <token>"), so every generated name is unique.
INSERT INTO "Teams"
    ("Id", "Name", "Coach", "City", "Type", "Division", "Category", "CreatedAt", "Active")
SELECT
    gen_random_uuid(),
    town || ' ' || (ARRAY['CF','CD','FC','Balompié','Deportivo'])[1 + ((i - 1) % 5)],
    (ARRAY['Javier','Miguel','Antonio','Luis','Carlos','Pablo','Sergio','David','Iker','Unai','Fran','Diego','Rubén','Adrián','Marcos'])[1 + (i % 15)]
        || ' ' ||
    (ARRAY['García','Fernández','Martínez','López','Sánchez','Romero','Torres','Navarro','Gil','Vidal','Cabrera','Moya','Ramos','Ibáñez','Serrano'])[1 + ((i * 7) % 15)],
    town,
    (ARRAY['Football11','Football11','Football11','Football8','Futsal','BeachSoccer'])[1 + (i % 6)],
    (ARRAY['HonorDivision','RegionalLeague','First','Second'])[1 + (i % 4)],
    (ARRAY['Chupetes','Prebenjamines','Benjamines','Alevines','Infantiles','Cadetes','Juveniles','Aficionados','Veteranos'])[1 + (i % 9)],
    now() - ((i * 2 + 5) || ' days')::interval,
    (i % 5 <> 0)
FROM (
    SELECT
        g.i,
        (ARRAY[
            'Marbella','Vélez-Málaga','Mijas','Fuengirola','Torremolinos',
            'Benalmádena','Estepona','Ronda','Antequera','Rincón de la Victoria',
            'Motril','Almuñécar','Baza','Guadix','Loja',
            'Maracena','Armilla','Albolote','Atarfe','Huétor Vega',
            'Lucena','Puente Genil','Montilla','Priego de Córdoba','Cabra',
            'Baena','Palma del Río','Pozoblanco','Peñarroya-Pueblonuevo','Fernán Núñez',
            'Jerez de la Frontera','Algeciras','San Fernando','El Puerto de Santa María','Chiclana de la Frontera',
            'La Línea de la Concepción','Puerto Real','Sanlúcar de Barrameda','Arcos de la Frontera','Barbate',
            'El Ejido','Roquetas de Mar','Níjar','Vícar','Adra',
            'Huércal-Overa','Berja','Vera','Cuevas del Almanzora','Carboneras',
            'Linares','Andújar','Úbeda','Martos','Alcalá la Real',
            'Bailén','Torredonjimeno','Mancha Real','Villacarrillo','Jódar'
        ])[1 + (g.i - 1) / 5] AS town
    FROM generate_series(1, 300) AS g(i)
) AS rows
ON CONFLICT ("Name") DO NOTHING;

COMMIT;

SELECT count(*) AS teams_total FROM "Teams";
