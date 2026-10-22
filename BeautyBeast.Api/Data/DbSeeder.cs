using BeautyBeast.Api.Entities;
using BeautyBeast.Api.Helpers;

namespace BeautyBeast.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Users.Any()) return;

        // ── Artist 1: Ana Popescu — Skin & Lashes ──────────────────────────
        var ana = new User
        {
            Username = "ana.beauty",
            Email = "ana@beautybeast.local",
            DisplayName = "Ana Popescu",
            PasswordHash = PasswordHelper.Hash("password123"),
            Role = UserRole.Artist,
            Bio = "Aesthetician focused on safe, evidence-based skincare.",
            AvatarUrl = "https://picsum.photos/seed/ana-beauty/200/200",
            CoverUrl = "https://picsum.photos/seed/ana-cover/1200/400",
            ArtistProfile = new ArtistProfile
            {
                Headline = "Master Aesthetician, London",
                Specialty = "Microblading, Lash Lift, Skin Care",
                Location = "London, UK",
                About = "10+ years working with clients across Europe. Passionate about safety standards and continuous education.",
                Experience = "Senior Aesthetician at Glow Studio (2020–Present)\nFreelance Beauty Artist (2017–2020)",
                Education = "Diploma in Aesthetic Treatments, London Beauty School (2017)\nAdvanced Microblading Certification, BrowArt Academy (2019)",
                Skills = "microblading, lash lift, dermaplaning, hydrafacial, consultation",
                AcceptingBookings = true
            },
            Achievements = new List<Achievement>
            {
                new() { Title = "Advanced Microblading Certification", Issuer = "BrowArt Academy", IssuedOn = new DateTime(2019, 6, 1) },
                new() { Title = "Skin Safety Workshop", Issuer = "British Beauty Council", IssuedOn = new DateTime(2022, 11, 10) },
            }
        };

        // ── Artist 2: Sophie Ramirez — Nail Artist ─────────────────────────
        var sophie = new User
        {
            Username = "sophie.nails",
            Email = "sophie@beautybeast.local",
            DisplayName = "Sophie Ramirez",
            PasswordHash = PasswordHelper.Hash("password123"),
            Role = UserRole.Artist,
            Bio = "Nail artist with a love for clean, minimal designs and healthy nail education.",
            AvatarUrl = "https://picsum.photos/seed/sophie-nails/200/200",
            CoverUrl = "https://picsum.photos/seed/sophie-cover/1200/400",
            ArtistProfile = new ArtistProfile
            {
                Headline = "Nail Artist & Educator, Manchester",
                Specialty = "Gel Manicure, Nail Art, Acrylic Extensions",
                Location = "Manchester, UK",
                About = "8 years in the nail industry. I believe healthy nails are beautiful nails — every treatment I do starts with nail health in mind. I love teaching clients how to maintain their nails between visits.",
                Experience = "Lead Nail Technician at Polish & Co (2021–Present)\nFreelance Nail Artist (2018–2021)\nSalon Technician, NailBar Manchester (2016–2018)",
                Education = "VTCT Level 3 Nail Technology (2016)\nGelosophy Gel System Certification (2018)\nAdvanced Nail Art Masterclass, The Gel Bottle Inc. (2022)",
                Skills = "gel manicure, nail art, acrylic extensions, nail health, cuticle care, nail prep",
                AcceptingBookings = true
            },
            Achievements = new List<Achievement>
            {
                new() { Title = "VTCT Level 3 Nail Technology", Issuer = "VTCT", IssuedOn = new DateTime(2016, 7, 1) },
                new() { Title = "Advanced Nail Art Masterclass", Issuer = "The Gel Bottle Inc.", IssuedOn = new DateTime(2022, 3, 15) },
                new() { Title = "Nail Health & Anatomy Certificate", Issuer = "Scratch Magazine Academy", IssuedOn = new DateTime(2023, 9, 10) },
            }
        };

        // ── Artist 3: Zara Hassan — Brows & Makeup ────────────────────────
        var zara = new User
        {
            Username = "zara.artistry",
            Email = "zara@beautybeast.local",
            DisplayName = "Zara Hassan",
            PasswordHash = PasswordHelper.Hash("password123"),
            Role = UserRole.Artist,
            Bio = "Brow specialist and makeup artist. I frame faces for a living.",
            AvatarUrl = "https://picsum.photos/seed/zara-artistry/200/200",
            CoverUrl = "https://picsum.photos/seed/zara-cover/1200/400",
            ArtistProfile = new ArtistProfile
            {
                Headline = "Brow Specialist & Makeup Artist, Birmingham",
                Specialty = "HD Brows, Brow Lamination, Bridal Makeup",
                Location = "Birmingham, UK",
                About = "Trained in HD Brows and advanced lamination techniques. Bridal makeup is my passion — I've worked on over 200 weddings across the UK. I teach clients brow mapping so they understand the process, not just the result.",
                Experience = "Head Brow Artist at The Brow Bar (2019–Present)\nFreelance Bridal MUA (2017–Present)\nMakeup Counter Artist, Selfridges Birmingham (2015–2019)",
                Education = "HD Brows Pro Training (2019)\nAdvanced Brow Lamination, Brow Code (2021)\nBridal Makeup Intensive, London College of Fashion (2017)",
                Skills = "HD brows, brow lamination, brow tinting, bridal makeup, airbrush foundation, lash application",
                AcceptingBookings = true
            },
            Achievements = new List<Achievement>
            {
                new() { Title = "HD Brows Pro Certification", Issuer = "HD Brows", IssuedOn = new DateTime(2019, 4, 20) },
                new() { Title = "Advanced Brow Lamination", Issuer = "Brow Code", IssuedOn = new DateTime(2021, 8, 5) },
                new() { Title = "Bridal Makeup Intensive", Issuer = "London College of Fashion", IssuedOn = new DateTime(2017, 6, 12) },
            }
        };

        // ── Demo client ────────────────────────────────────────────────────
        var client = new User
        {
            Username = "mia.client",
            Email = "mia@beautybeast.local",
            DisplayName = "Mia Johnson",
            PasswordHash = PasswordHelper.Hash("password123"),
            Role = UserRole.Client,
            Bio = "Looking after my skin and learning along the way.",
            AvatarUrl = "https://picsum.photos/seed/mia-client/200/200",
            ClientProfile = new ClientProfile
            {
                Location = "London, UK",
                Interests = "skincare, lashes, brows",
                About = "Curious about evidence-based beauty. I read before I book."
            }
        };

        db.Users.AddRange(ana, sophie, zara, client);
        await db.SaveChangesAsync();

        // ── Working hours ──────────────────────────────────────────────────
        // Ana: Mon–Fri 10:00–17:00, lunch 12:00–13:00
        foreach (var d in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
        {
            db.WorkingDays.Add(new WorkingDay { ArtistId = ana.Id, Day = d, StartMinutes = 600, EndMinutes = 1020, BreakStartMinutes = 720, BreakEndMinutes = 780 });
        }

        // Sophie: Tue–Sat 09:00–18:00, lunch 13:00–14:00
        foreach (var d in new[] { DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday })
        {
            db.WorkingDays.Add(new WorkingDay { ArtistId = sophie.Id, Day = d, StartMinutes = 540, EndMinutes = 1080, BreakStartMinutes = 780, BreakEndMinutes = 840 });
        }

        // Zara: Mon, Wed–Sat 10:00–19:00, lunch 13:00–14:00
        foreach (var d in new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday })
        {
            db.WorkingDays.Add(new WorkingDay { ArtistId = zara.Id, Day = d, StartMinutes = 600, EndMinutes = 1140, BreakStartMinutes = 780, BreakEndMinutes = 840 });
        }

        // ── Treatments ─────────────────────────────────────────────────────
        // Ana
        db.Treatments.AddRange(
            new Treatment { ArtistId = ana.Id, Name = "Microblading Full Session", Category = "Brows", Description = "Semi-permanent eyebrow technique creating natural-looking hair strokes. Includes a full consultation, mapping, and application. Touch-up recommended at 6–8 weeks.", Price = 280m, DurationMinutes = 120, ImageUrl = "https://picsum.photos/seed/microblading/600/400" },
            new Treatment { ArtistId = ana.Id, Name = "Microblading Touch-Up", Category = "Brows", Description = "Follow-up session at 6–8 weeks to perfect healed results. For existing clients only.", Price = 100m, DurationMinutes = 75, ImageUrl = "https://picsum.photos/seed/microblading-touch/600/400" },
            new Treatment { ArtistId = ana.Id, Name = "Lash Lift & Tint", Category = "Lashes", Description = "Lifts and curls natural lashes from the root, with an optional tint for deeper definition. Results last 6–8 weeks.", Price = 65m, DurationMinutes = 60, ImageUrl = "https://picsum.photos/seed/lash-lift/600/400" },
            new Treatment { ArtistId = ana.Id, Name = "Hydrafacial", Category = "Skin", Description = "Multi-step treatment that cleanses, exfoliates, extracts, and infuses skin with serums. Suitable for all skin types.", Price = 95m, DurationMinutes = 60, ImageUrl = "https://picsum.photos/seed/hydrafacial/600/400" },
            new Treatment { ArtistId = ana.Id, Name = "Dermaplaning", Category = "Skin", Description = "Manual exfoliation using a sterile blade to remove dead skin cells and vellus hair. Instantly smoother skin.", Price = 70m, DurationMinutes = 45, ImageUrl = "https://picsum.photos/seed/dermaplaning/600/400" }
        );

        // Sophie
        db.Treatments.AddRange(
            new Treatment { ArtistId = sophie.Id, Name = "Classic Manicure", Category = "Nails", Description = "File, shape, cuticle care, hand massage, and polish finish with regular lacquer.", Price = 30m, DurationMinutes = 45, ImageUrl = "https://picsum.photos/seed/classic-mani/600/400" },
            new Treatment { ArtistId = sophie.Id, Name = "Gel Manicure", Category = "Nails", Description = "Long-lasting gel polish application with full nail prep. Includes cuticle work, base, colour, and top coat under UV lamp. Lasts 2–3 weeks.", Price = 45m, DurationMinutes = 60, ImageUrl = "https://picsum.photos/seed/gel-mani/600/400" },
            new Treatment { ArtistId = sophie.Id, Name = "Gel Removal & Rebooking", Category = "Nails", Description = "Safe gel soak-off with nail health assessment. Price includes a fresh gel manicure when rebooked in the same session.", Price = 55m, DurationMinutes = 75, ImageUrl = "https://picsum.photos/seed/gel-removal/600/400" },
            new Treatment { ArtistId = sophie.Id, Name = "Nail Art (per design)", Category = "Nail Art", Description = "Bespoke nail art applied over gel or regular polish. Price per nail or full set — discuss your ideas at booking.", Price = 65m, DurationMinutes = 90, ImageUrl = "https://picsum.photos/seed/nail-art/600/400" },
            new Treatment { ArtistId = sophie.Id, Name = "Acrylic Extensions (Full Set)", Category = "Nails", Description = "Full set of sculpted acrylic extensions. Includes nail prep, tip or form application, overlay, shaping, and gel polish finish.", Price = 80m, DurationMinutes = 120, ImageUrl = "https://picsum.photos/seed/acrylic-nails/600/400" }
        );

        // Zara
        db.Treatments.AddRange(
            new Treatment { ArtistId = zara.Id, Name = "HD Brows", Category = "Brows", Description = "Full HD Brows treatment: brow mapping, wax, tint, thread, and styling. Tailored to your face shape and natural growth.", Price = 48m, DurationMinutes = 75, ImageUrl = "https://picsum.photos/seed/hd-brows/600/400" },
            new Treatment { ArtistId = zara.Id, Name = "Brow Lamination", Category = "Brows", Description = "A chemical process that sets brow hairs in place for a full, brushed-up look. Results last 6–8 weeks. Includes brow tint.", Price = 55m, DurationMinutes = 60, ImageUrl = "https://picsum.photos/seed/brow-lamination/600/400" },
            new Treatment { ArtistId = zara.Id, Name = "Brow Tint & Wax", Category = "Brows", Description = "Quick tint to add depth and definition, followed by a clean wax shaping. Perfect for maintenance between fuller treatments.", Price = 28m, DurationMinutes = 30, ImageUrl = "https://picsum.photos/seed/brow-tint/600/400" },
            new Treatment { ArtistId = zara.Id, Name = "Bridal Makeup Trial", Category = "Makeup", Description = "Full bridal look trial with consultation. We test looks, lighting, and longevity. Highly recommended 4–8 weeks before the wedding day.", Price = 90m, DurationMinutes = 120, ImageUrl = "https://picsum.photos/seed/bridal-trial/600/400" },
            new Treatment { ArtistId = zara.Id, Name = "Bridal Makeup (Wedding Day)", Category = "Makeup", Description = "Wedding day makeup application. Includes skin prep, full makeup, and a travel touch-up kit. Travel within 30 miles included.", Price = 175m, DurationMinutes = 150, ImageUrl = "https://picsum.photos/seed/bridal-makeup/600/400" }
        );

        // ── Library (Resources) ────────────────────────────────────────────
        // Ana
        db.Resources.AddRange(
            new Resource
            {
                AuthorId = ana.Id,
                Title = "Patch testing 101: why we always do it",
                Category = "Skin Safety",
                Kind = ResourceKind.Article,
                Body = "A patch test reveals sensitivity to a product 24–48 hours before treatment. It's quick, cheap, and prevents serious reactions. We document the result, the date, and the product batch.\n\nA practitioner who skips patch testing is taking on risk that should sit with no one.",
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new Resource
            {
                AuthorId = ana.Id,
                Title = "Sterilisation standards in a small studio",
                Category = "Sterilisation",
                Kind = ResourceKind.Article,
                Body = "Single-use needles. Autoclaved tools. Surface disinfection between clients. A logbook for everything. None of it is optional.\n\nIf a studio can't show you their sterilisation log on request, that tells you something.",
                CreatedAt = DateTime.UtcNow.AddDays(-50)
            },
            new Resource
            {
                AuthorId = ana.Id,
                Title = "Aftercare for microblading",
                Category = "Aftercare",
                Kind = ResourceKind.Article,
                Body = "Days 1–7: keep the area dry. No makeup on the brows. Apply the supplied balm thinly twice a day. Avoid sun, sweat, saunas, and chlorine.\n\nDays 7–14: light flaking is completely normal — this is the pigment settling. Do not pick or rub. The colour will appear lighter once healed.\n\nWeek 6–8: attend your touch-up to perfect the result.",
                CreatedAt = DateTime.UtcNow.AddDays(-45)
            },
            new Resource
            {
                AuthorId = ana.Id,
                Title = "What is dermaplaning and is it right for you?",
                Category = "Skin",
                Kind = ResourceKind.Article,
                Body = "Dermaplaning uses a sterile, single-use surgical blade at a 45-degree angle to remove the top layer of dead skin cells and vellus hair (peach fuzz).\n\nWho it's for: most skin types, including sensitive skin. Particularly good for those with dry, dull, or uneven texture.\n\nWho it's not for: active acne, rosacea flare-ups, or very thin/sensitive skin post-retinol.",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        );

        // Sophie
        db.Resources.AddRange(
            new Resource
            {
                AuthorId = sophie.Id,
                Title = "Nail anatomy: what lies beneath the plate",
                Category = "Nail Education",
                Kind = ResourceKind.Article,
                Body = "The nail plate is just the visible tip of the iceberg. Below it: the nail bed (living tissue), the matrix (where growth originates), and the lunula (the pale half-moon at the base).\n\nHealthy nail growth comes from a healthy matrix. Damage here — from heavy-handed filing or lifting — can cause permanent ridging or discolouration.\n\nAs a technician, my job is to protect the matrix while delivering the look you want.",
                CreatedAt = DateTime.UtcNow.AddDays(-55)
            },
            new Resource
            {
                AuthorId = sophie.Id,
                Title = "How to care for your gel manicure",
                Category = "Aftercare",
                Kind = ResourceKind.Article,
                Body = "Your gel manicure will last 2–3 weeks with the right care.\n\n✔ Wear gloves for cleaning and washing up — prolonged water exposure is the number one cause of lifting.\n✔ Apply cuticle oil daily. It keeps the skin supple and extends the life of the product edge.\n✔ Avoid using your nails as tools.\n\nWhen it's time to remove: come back to the salon. Home peeling causes nail thinning and is the reason most clients think gel damaged their nails — the gel didn't. The removal did.",
                CreatedAt = DateTime.UtcNow.AddDays(-40)
            },
            new Resource
            {
                AuthorId = sophie.Id,
                Title = "Choosing the right nail shape for your hands",
                Category = "Nail Care",
                Kind = ResourceKind.Article,
                Body = "Nail shape affects how your hands look as much as the colour does.\n\nOval & almond: lengthen shorter fingers. The most universally flattering shape.\nSquare: suits longer, slender fingers. Looks elegant with minimal art.\nCoffin/Ballerina: bold and dramatic. Works best with extensions.\nRound: low-maintenance and practical. Great for short nails.\nStiletto: statement shape. Best with acrylics for durability.\n\nI always assess hand shape and finger length at consultation — never skip this step.",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new Resource
            {
                AuthorId = sophie.Id,
                Title = "The truth about gel and nail damage",
                Category = "Nail Health",
                Kind = ResourceKind.Article,
                Body = "Gel doesn't damage nails. Incorrect application and removal does.\n\nThe most common culprits:\n\n1. Over-filing the natural nail during prep\n2. Peeling or picking gel off (removes layers of the nail plate)\n3. UV exposure without proper curing\n4. Using MMA (methyl methacrylate) monomer — a cheap and harmful acrylic ingredient still used by some salons\n\nHow to protect yourself: ask your technician what products they use. HEMA-free gels are a safer choice for sensitive clients. Always remove product professionally.",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        );

        // Zara
        db.Resources.AddRange(
            new Resource
            {
                AuthorId = zara.Id,
                Title = "Brow lamination: the 48-hour rules",
                Category = "Aftercare",
                Kind = ResourceKind.Article,
                Body = "Brow lamination sets the brow hairs in a new position using a gentle chemical process. The result takes 24–48 hours to fully set.\n\nFor the first 48 hours:\n✘ No water on the brows\n✘ No steam or saunas\n✘ No makeup or skincare on the brow area\n✘ Do not brush against the direction of the set\n\nAfter 48 hours:\n✔ Apply nourishing brow serum or castor oil nightly\n✔ Brush brows upward each morning with a spoolie\n✔ Avoid oil-based removers directly on the brows — they break down the set faster",
                CreatedAt = DateTime.UtcNow.AddDays(-52)
            },
            new Resource
            {
                AuthorId = zara.Id,
                Title = "Understanding brow mapping",
                Category = "Brow Education",
                Kind = ResourceKind.Article,
                Body = "Brow mapping is the technique of measuring natural brow proportions against facial features before any hair removal or tinting.\n\nThe three key points:\n• Start: aligned with the outer edge of the nostril\n• Arch: above the outer edge of the iris\n• End: angled from nostril past the outer corner of the eye\n\nThese are guidelines, not rules. Every face is different. I map brows on every client, every time — even for a maintenance visit. It takes two minutes and prevents asymmetry.",
                CreatedAt = DateTime.UtcNow.AddDays(-38)
            },
            new Resource
            {
                AuthorId = zara.Id,
                Title = "Skin prep before your makeup appointment",
                Category = "Makeup Tips",
                Kind = ResourceKind.Article,
                Body = "Good makeup starts with good skin prep. Here's what to do the week before your appointment:\n\n3–5 days before: avoid starting any new skincare products, especially actives (retinol, acids, vitamin C serums). New products can cause unexpected reactions.\n\nThe night before: gently exfoliate with a washcloth. Moisturise well. Skip heavy oils that can cause foundation to slip.\n\nThe morning of: cleanse, tone, and moisturise as normal. Apply SPF if the appointment is during the day — but let it absorb fully before arriving. Skip primer — I'll apply everything from a clean base.",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Resource
            {
                AuthorId = zara.Id,
                Title = "Bridal makeup timeline: when to book what",
                Category = "Bridal",
                Kind = ResourceKind.Article,
                Body = "Planning your bridal beauty calendar:\n\n12 months out: research and enquire with MUAs. Top artists book up fast for peak season (May–September).\n\n9–12 months out: confirm your booking with a deposit. Many artists hold limited Saturday slots.\n\n4–8 weeks before: complete your makeup trial. This gives time to adjust looks, test longevity, and try a second style if needed.\n\n2 weeks before: avoid facials, peels, and waxing. Let your skin settle.\n\nWedding morning: eat breakfast. Have hair done first. Expect makeup to take 60–90 minutes for the bride alone.",
                CreatedAt = DateTime.UtcNow.AddDays(-14)
            }
        );

        // ── Consent Forms ──────────────────────────────────────────────────
        // Global templates (no owner)
        db.ConsentForms.AddRange(
            new ConsentForm
            {
                Title = "Microblading Consent Form (Template)",
                Category = "Microblading",
                IsTemplate = true,
                Body = "I, the undersigned, confirm that I have been informed of the procedure, its risks, expected outcomes, and aftercare requirements. I confirm I am not pregnant, not on blood thinners, and have no known allergies to pigments or anaesthetic creams.\n\nClient signature: ____________________  Date: ____________\nPractitioner signature: _______________  Date: ____________",
                CreatedAt = DateTime.UtcNow.AddDays(-90)
            },
            new ConsentForm
            {
                Title = "General Beauty Treatment Consent (Template)",
                Category = "General",
                IsTemplate = true,
                Body = "I confirm that the practitioner has explained the treatment, expected results, possible risks and side effects, and aftercare. I have disclosed all relevant medical history and medications. I understand results may vary.\n\nClient signature: ____________________  Date: ____________",
                CreatedAt = DateTime.UtcNow.AddDays(-90)
            }
        );

        // Sophie's forms
        db.ConsentForms.AddRange(
            new ConsentForm
            {
                OwnerId = sophie.Id,
                Title = "Gel & Acrylic Nail Extensions — Client Consent",
                Category = "Nails",
                IsTemplate = false,
                Body = "Treatment: Gel / Acrylic Nail Extensions\nArtist: Sophie Ramirez\n\nI confirm that I:\n• Have no known allergies to acrylate-based products (gel or acrylic monomer)\n• Am not currently taking medication that thins the blood or affects nail health\n• Have disclosed any nail conditions (onycholysis, fungal infection, psoriasis) to my technician\n• Understand that gel and acrylic extensions require professional removal and I will not pick, peel, or bite the product off\n• Have been advised that results vary with natural nail condition and aftercare compliance\n\nI consent to the treatment as described and confirm the above information is accurate.\n\nClient name: ______________________  Date: ____________\nClient signature: __________________\nTechnician signature: ______________",
                CreatedAt = DateTime.UtcNow.AddDays(-45)
            },
            new ConsentForm
            {
                OwnerId = sophie.Id,
                Title = "Nail Art & Enhancement — Patch Test Declaration",
                Category = "Nails",
                IsTemplate = false,
                Body = "Product sensitivity declaration for nail services.\n\nClient name: ____________________________\nDate of patch test (if applicable): ___________\nProduct(s) tested: _________________________\nReaction observed: □ None  □ Mild redness  □ Irritation  □ Other: ____________\n\nI confirm that I have been offered a patch test and understand the risks of proceeding without one if declined.\n\nClient signature: __________________  Date: ____________\nTechnician: Sophie Ramirez",
                CreatedAt = DateTime.UtcNow.AddDays(-40)
            }
        );

        // Zara's forms
        db.ConsentForms.AddRange(
            new ConsentForm
            {
                OwnerId = zara.Id,
                Title = "Brow Lamination — Client Consent & Health Declaration",
                Category = "Brow Lamination",
                IsTemplate = false,
                Body = "Treatment: Brow Lamination (with optional tint)\nArtist: Zara Hassan\n\nContraindications — I confirm I do NOT currently have:\n□ Active eczema, psoriasis, or dermatitis in the brow area\n□ A known allergy to peroxide, thioglycolate, or hydrogen peroxide\n□ Sunburn or broken skin around the brow area\n□ Chemotherapy treatment within the last 3 months\n□ Pregnancy (treatment not recommended in first trimester)\n\nI have completed a patch test at least 24 hours before this appointment: □ Yes  □ Declined (initial here: ___)\n\nI understand:\n• Results last approximately 6–8 weeks\n• I must follow the 48-hour aftercare rules provided\n• Results depend on natural brow hair texture and condition\n\nClient signature: __________________  Date: ____________\nArtist signature: __________________  Date: ____________",
                CreatedAt = DateTime.UtcNow.AddDays(-50)
            },
            new ConsentForm
            {
                OwnerId = zara.Id,
                Title = "Bridal Makeup — Service Agreement & Consent",
                Category = "Bridal",
                IsTemplate = false,
                Body = "Bridal Makeup Service Agreement\nArtist: Zara Hassan\n\nThis form confirms the terms of your bridal makeup booking.\n\nService booked: □ Trial  □ Wedding Day  □ Both\nDate of service: ____________________________\nVenue / location: ___________________________\nAgreed call time: ___________________________\n\nSkin & health disclosure:\n• Known skin conditions or sensitivities: _______________________\n• Regular skincare/medication that may affect the skin: __________\n• Product allergies: ________________________________________\n\nI confirm that:\n• A trial has been completed (or I understand results may vary without one)\n• I have provided accurate health and skin information\n• Touch-up products for the day have been agreed\n• I understand the deposit and cancellation policy\n\nClient signature: __________________  Date: ____________\nArtist signature: __________________  Date: ____________",
                CreatedAt = DateTime.UtcNow.AddDays(-35)
            }
        );

        await db.SaveChangesAsync();

        // ── Posts ──────────────────────────────────────────────────────────
        db.Posts.AddRange(
            // Ana's posts
            new Post
            {
                AuthorId = ana.Id,
                Title = "Why your skin barrier matters more than any serum",
                Body = "I get asked constantly which serum is 'the best one'. The honest answer: the best serum in the world won't help if your skin barrier is compromised.\n\nThe skin barrier — the outermost layer — regulates hydration, protects against pathogens, and keeps irritants out. When it's disrupted (over-exfoliation, harsh cleansers, extreme weather), skin becomes reactive, dry, and prone to breakouts.\n\nMy top 3 barrier-supporting habits:\n\n1. Stop washing your face with hot water. Lukewarm only.\n2. Moisturise immediately after cleansing — within 60 seconds, while skin is still damp.\n3. Take a one-week break from all actives if your skin feels sensitised. Less is often more.\n\nFix the barrier first. Then we talk serums.",
                Kind = PostKind.Article,
                CreatedAt = DateTime.UtcNow.AddDays(-28)
            },
            new Post
            {
                AuthorId = ana.Id,
                Title = null,
                Body = "Fresh lash lift from this morning. 60 minutes, no extensions, just your own lashes lifted from the root. The difference is always more dramatic than clients expect.",
                MediaItems = new List<PostMedia> { new() { Url = "https://picsum.photos/seed/lash-result/800/600", SortOrder = 0 } },
                Kind = PostKind.Image,
                CreatedAt = DateTime.UtcNow.AddDays(-14)
            },
            new Post
            {
                AuthorId = ana.Id,
                Title = "A note on practitioner qualifications",
                Body = "In the UK, there is currently no legal requirement to hold a qualification before performing many beauty treatments — including microblading and lash treatments. This is changing, but slowly.\n\nWhat you can check:\n• Insurance (ask for their policy number and verify)\n• Training certificates and the awarding body\n• Portfolio with healed results (not just fresh)\n• Whether they use single-use needles (non-negotiable for microblading)\n\nA good practitioner will welcome these questions. Don't apologise for asking.",
                Kind = PostKind.Article,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },

            // Sophie's posts
            new Post
            {
                AuthorId = sophie.Id,
                Title = null,
                Body = "Today's set — negative space geo art over natural nails. No extensions, no length added. Sometimes the best canvas is what you already have.",
                MediaItems = new List<PostMedia>
                {
                    new() { Url = "https://picsum.photos/seed/nail-geo/800/600", SortOrder = 0 },
                    new() { Url = "https://picsum.photos/seed/nail-geo-2/800/600", SortOrder = 1 },
                    new() { Url = "https://picsum.photos/seed/nail-geo-3/800/600", SortOrder = 2 }
                },
                Kind = PostKind.Image,
                CreatedAt = DateTime.UtcNow.AddDays(-22)
            },
            new Post
            {
                AuthorId = sophie.Id,
                Title = "Nail prep: the step nobody talks about",
                Body = "Every nail professional knows that nail prep is 80% of the job. But clients rarely hear about it.\n\nHere's what happens before I apply a single drop of gel:\n\n1. Remove any existing product safely (soak-off, never drill through)\n2. Assess nail plate health — look for lifting, ridging, thinning, or signs of infection\n3. Gently push back (not cut) the cuticle\n4. Lightly buff the nail plate to create adhesion — just enough, never over-filing\n5. Dehydrate and prime — removes moisture and oils that cause lifting\n\nSkip any of these steps and the product will fail. It's not the gel. It's the prep.",
                Kind = PostKind.Article,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Post
            {
                AuthorId = sophie.Id,
                Title = null,
                Body = "Autumn set 🍂 Terracotta, burnt orange, and a hint of gold foil. The gel colours this season are everything.",
                MediaItems = new List<PostMedia>
                {
                    new() { Url = "https://picsum.photos/seed/autumn-nails/800/600", SortOrder = 0 },
                    new() { Url = "https://picsum.photos/seed/autumn-nails-2/800/600", SortOrder = 1 }
                },
                Kind = PostKind.Image,
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            },

            // Zara's posts
            new Post
            {
                AuthorId = zara.Id,
                Title = null,
                Body = "Before and after brow lamination — same client, same lighting, zero filters. This is what 45 minutes can do.",
                MediaItems = new List<PostMedia>
                {
                    new() { Url = "https://picsum.photos/seed/brow-before/800/600", SortOrder = 0 },
                    new() { Url = "https://picsum.photos/seed/brow-after/800/600", SortOrder = 1 }
                },
                Kind = PostKind.Image,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Post
            {
                AuthorId = zara.Id,
                Title = "Why I always patch test — even repeat clients",
                Body = "I've had clients who've had brow tints with me for two years suddenly develop a reaction. Allergies can develop at any point — even to products you've tolerated before.\n\nThis is why I patch test every client, every time for tinting services. It adds 24 hours to the process. That's the deal.\n\nIf a practitioner tells you a patch test isn't necessary because you've had the treatment before: walk away. That's not a convenience — it's a liability being passed onto you.",
                Kind = PostKind.Article,
                CreatedAt = DateTime.UtcNow.AddDays(-12)
            },
            new Post
            {
                AuthorId = zara.Id,
                Title = null,
                Body = "Bridal season wrap-up. 14 weddings in 8 weeks. Every face is different, every brief is different — that's what keeps this job interesting.",
                MediaItems = new List<PostMedia>
                {
                    new() { Url = "https://picsum.photos/seed/bridal-season/800/600", SortOrder = 0 },
                    new() { Url = "https://picsum.photos/seed/bridal-season-2/800/600", SortOrder = 1 },
                    new() { Url = "https://picsum.photos/seed/bridal-season-3/800/600", SortOrder = 2 }
                },
                Kind = PostKind.Image,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        );

        // ── Statuses ───────────────────────────────────────────────────────
        db.Statuses.AddRange(
            // Ana
            new StatusUpdate { AuthorId = ana.Id, Text = "Reminder: if a treatment seems unusually cheap, ask why. Your face and skin are worth the research before you book.", CreatedAt = DateTime.UtcNow.AddDays(-32) },
            new StatusUpdate { AuthorId = ana.Id, Text = "October availability now open. A few microblading slots left for the month — link in profile to book.", CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new StatusUpdate { AuthorId = ana.Id, Text = "Just finished a full day of hydrafacials. Every single client had noticeably congested skin. Drink more water, wear SPF, and please stop using wipes as your only cleanse.", CreatedAt = DateTime.UtcNow.AddDays(-9) },

            // Sophie
            new StatusUpdate { AuthorId = sophie.Id, Text = "PSA: cuticle oil is not optional. It's the cheapest thing you can do to extend the life of your manicure and keep your nails healthy. Use it daily.", CreatedAt = DateTime.UtcNow.AddDays(-26) },
            new StatusUpdate { AuthorId = sophie.Id, Text = "New gel palette just landed — 48 new shades for autumn/winter. Swatches going up on Instagram this weekend. The dusty lilac is going to be everywhere.", CreatedAt = DateTime.UtcNow.AddDays(-16) },
            new StatusUpdate { AuthorId = sophie.Id, Text = "Saturday slots are fully booked through to end of November. Tuesday and Wednesday mornings still have availability if you're flexible!", CreatedAt = DateTime.UtcNow.AddDays(-7) },

            // Zara
            new StatusUpdate { AuthorId = zara.Id, Text = "Brow lamination is not permanent. It lasts 6–8 weeks. Please stop expecting it to survive swimming, steaming, and oil cleansers. The aftercare matters.", CreatedAt = DateTime.UtcNow.AddDays(-29) },
            new StatusUpdate { AuthorId = zara.Id, Text = "Just passed my advanced HD Brows refresher. Updated techniques, updated training log. Always learning.", CreatedAt = DateTime.UtcNow.AddDays(-17) },
            new StatusUpdate { AuthorId = zara.Id, Text = "Bridal enquiries for 2026 are already coming in. If you're getting married next year, don't leave it — the good dates go fast. DM me to check availability.", CreatedAt = DateTime.UtcNow.AddDays(-4) }
        );

        await db.SaveChangesAsync();
    }
}
