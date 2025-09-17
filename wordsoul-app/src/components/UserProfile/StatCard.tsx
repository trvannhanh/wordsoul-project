import { icons } from "./Icon";


interface StatCardProps {
  label: string;
  value: number;
}

const StatCard: React.FC<StatCardProps> = ({ label, value }) => (
  <div className="flex items-center gap-2">
    {icons[label]}
    <div className="flex-col">
      <div className="text-sm font-pokemon text-yellow-300">{value}</div>
      <div className="text-xs text-gray-300">{label}</div>
    </div>
  </div>
);

export default StatCard;