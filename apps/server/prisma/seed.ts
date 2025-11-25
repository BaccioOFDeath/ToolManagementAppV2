import { PrismaClient } from '@prisma/client';
import { seedDemoOrgStructure } from './seeds/seed-demo-org-structure';
import { seedSystemRoles } from './seeds/seed-system-roles';

async function main() {
  const prisma = new PrismaClient();

  try {
    console.log('Running Prisma seeders for SDAutoOS organizational hierarchy...');
    const roleMap = await seedSystemRoles(prisma);
    await seedDemoOrgStructure(prisma, roleMap);
    console.log('Seeding completed successfully.');
  } catch (error) {
    console.error('Seeding failed:', error);
    process.exitCode = 1;
  } finally {
    await prisma.$disconnect();
  }
}

main();
