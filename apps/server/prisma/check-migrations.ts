import path from 'node:path';
import { execSync } from 'node:child_process';

const schemaPath = path.join(__dirname, 'schema.prisma');

function getStatus() {
  try {
    const output = execSync(`npx prisma migrate status --schema ${schemaPath} --json`, {
      stdio: ['ignore', 'pipe', 'pipe'],
      encoding: 'utf-8',
    });
    return JSON.parse(output);
  } catch (error: any) {
    const stderr = error?.stderr?.toString?.() ?? '';
    console.error('Unable to read Prisma migration status.');
    if (stderr) {
      console.error(stderr.trim());
    }
    console.error('Ensure dependencies are installed and the DATABASE_URL is set.');
    process.exitCode = 1;
    return null;
  }
}

function printInstructions(pendingNames: string[]) {
  console.log('Detected unapplied migrations:');
  pendingNames.forEach((name) => console.log(` • ${name}`));
  console.log('\nNext steps:');
  console.log(`  1) Review the SQL at ${path.relative(process.cwd(), schemaPath)} and ./migrations/<name>/migration.sql`);
  console.log('  2) Apply locally with: npm run prisma:migrate-dev');
  console.log('  3) For production, use: npx prisma migrate deploy --schema ' + schemaPath);
}

const status = getStatus();

if (!status) {
  process.exit(1);
}

const pending = status?.pendingMigrationNames ?? [];

if (pending.length === 0) {
  console.log('All Prisma migrations are already applied.');
} else {
  printInstructions(pending);
}
