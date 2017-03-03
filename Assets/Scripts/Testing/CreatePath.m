function [path, inflectionPts] = CreatePath(origin, facing, depth, bearing, inflectionRate, stepSize)
% Creates a path of constant bearing which is subject to random inflection.
% Returns the path and inflection points (w/ angle).
% Return:
%   path    - 2xN vector of N pts on the path; [x, y]^-1
%   inflection points - 4xM vector of M inflection pts on the path;
%                       [x, y, theta, sign(dTheta)]
%
% Parameters:
%   origin  - starting location; [x,y]
%   facing  - starting angle (0° == East == [+1, 0])
%   depth   - length of path; meters
%   bearing - deflection rate from path tangent (°/meter)
%   inflectionRate - how often inflection points occur (%/meter)
%   stepSize - length of incremental steps (meter)

        % Allocate storage.
        path = zeros(2, round(depth * 1/stepSize));
        path(:,1) = origin(:,1);
        inflectionPts = zeros(4, round(depth * 1/stepSize));
        inflectionIdx = 0;
        
        % Calculate parameters.
        inflectionChance = inflectionRate * stepSize;
        dTheta = bearing * stepSize;
        theta = facing;
        
        % Compute essential path.
        for i=2:round(depth * 1/stepSize)
            if (rand < inflectionChance)
                dTheta = -dTheta;
                inflectionIdx = inflectionIdx + 1;
                inflectionPts(1:2, inflectionIdx) = path(:,i-1);
                inflectionPts(3, inflectionIdx) = theta;
                inflectionPts(4, inflectionIdx) = -sign(dTheta);
            end

            theta = theta + dTheta;
            path(1,i) = stepSize * cos(theta) + path(1,i-1);
            path(2,i) = stepSize * sin(theta) + path(2,i-1);
        end
        
        % Prune unused inflection points.
        inflectionPts = inflectionPts(:,1:inflectionIdx);
end