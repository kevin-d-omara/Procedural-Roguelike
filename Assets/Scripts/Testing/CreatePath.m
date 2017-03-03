function [path, inflectionPts] = CreatePath(origin, facing, depth, bearing, inflectionRate, stepSize)
% Creates a path of constant bearing which is subject to random inflection.
% Returns the path and inflection points.
%   origin  - starting location; [x,y]
%   facing  - starting angle (0° == East == [+1, 0])
%   depth   - length of path; meters
%   bearing - deflection rate from path tangent (°/meter)
%   inflectionRate - how often inflection points occur (%/meter)
%   stepSize - length of incremental steps (meter)

        % Allocate storage
        path = zeros(2, round(depth * 1/stepSize));
        path(:,1) = origin(:,1);
        inflectionPts = zeros(2, round(depth * 1/stepSize));
        inflectionIdx = 0;
        
        
        
        inflectionChance = inflectionRate * stepSize;
        dTheta = bearing * stepSize;
        theta = facing;
        
        % Compute essential path.
        for i=2:round(depth * 1/stepSize)
            if (rand < inflectionChance)
                dTheta = -dTheta;
                inflectionIdx = inflectionIdx + 1;
                inflectionPts(:, inflectionIdx) = path(:,i-1);
            end

            theta = theta + dTheta;
            path(1,i) = stepSize * cos(theta) + path(1,i-1);
            path(2,i) = stepSize * sin(theta) + path(2,i-1);
        end
        
        inflectionPts = inflectionPts(:,1:inflectionIdx);
end